﻿using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;

using FsCheck;
using FsCheck.Xunit;

using static Bouchonnois.Tests.Builders.PartieDeChasseDataBuilder;
using static Bouchonnois.Tests.Builders.ChasseurBuilder;
using static Bouchonnois.Tests.Assertions.PartieDeChasseExtensions;
using Microsoft.FSharp.Collections;

namespace Bouchonnois.Tests.Service;

public class TirerTest : PartieDeChasseServiceTests
{
    // Propriete de la partie de chasse 
    //---------------------------------------
    //  Pour n'importe quelle partie de chasse
    //  Quand un chasseur sans balle tire => Echec

    [Property]
    public Property PourNimporteQuellePartieDeChasseQuandUnChasseurSansBalleTireCestUnEchec()
    {
        return Prop.ForAll(TerrainGenerator(), GroupeDeChasseursGenerator(), (terrain, chasseurs) =>
        {
            var id = Guid.NewGuid();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.Add(new PartieDeChasse
            {
                Id = id,
                Chasseurs = chasseurs.ToList(),
                Terrain = terrain,
                Status = PartieStatus.EnCours,
                Events = [],
            });

            var service = new PartieDeChasseService(repository, TimeProvider);

            try
            {
                service.Tirer(id, chasseurs.Head.Nom);
            }
            catch ( TasPlusDeBallesMonVieuxChasseALaMain e )
            {
                return true;
            }

            return false;
        });
    }

    private Arbitrary<Terrain> TerrainGenerator()
    {
        return (from nom in Arb.Generate<string>()
                    // A minima 1 galinette sur le terrain
                from nbGalinette in Gen.Choose(1, int.MaxValue)
                select (new Terrain { Nom = nom, NbGalinettes = nbGalinette })).ToArbitrary();
    }

    private static Arbitrary<Chasseur> ChasseurGenerator()
        => (from nom in Arb.Generate<string>()
                // A minima 1 balle
            from nbBalles in Gen.Choose(0, 0)
            select new Chasseur() { Nom = nom, BallesRestantes = nbBalles }).ToArbitrary();

    private static Arbitrary<FSharpList<Chasseur>> GroupeDeChasseursGenerator()
        => // On définit le nombre de chasseurs dans le groupe [1; 1000]
        (from nbChasseurs in Gen.Choose(1, 1_000)
             // On utilise le nombre de chasseurs pour générer le bon nombre de chasseurs
         select ChasseurGenerator().Generator.Sample(1, nbChasseurs)).ToArbitrary();

    [Fact]
    public void AvecUnChasseurAyantDesBalles()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var nbGalinettes = 2;
        var unePartieDeChasseAvecDesChasseursAyantDesBalles = UnePartieDeChasse()
            .AvecDesChasseursAyantDesBalles(Dédé(), Bernard(), Robert())
            .EtUnTerrainAvecDesGalinettes(nbGalinettes)
            .Build();

        repository.Add(unePartieDeChasseAvecDesChasseursAyantDesBalles);
        var service = new PartieDeChasseService(repository, TimeProvider);

        service.Tirer(unePartieDeChasseAvecDesChasseursAyantDesBalles.Id, Chasseurs.Bernard);

        repository
            .HasSavedPartieDeChasse()
            .Should()
            .HasEmittedEvent(Now, $"{Chasseurs.Bernard} tire")
            .And
            .ChasseurATiré(Chasseurs.Bernard, expectedBallesRestantes: 7)
            .And
            .TerrainAEncoreDesGalinettes(nbGalinettes);
    }

    [Fact]
    public void EchoueAvecUnChasseurNayantPlusDeBalles()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 0,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.EnCours,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action tirerSansBalle = () => service.Tirer(id, "Bernard");

        tirerSansBalle.Should()
            .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.EnCours,
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        var nomChasseurInconnu = "Chasseur inconnu";

        Action? chasseurInconnuVeutTirer = () => service.Tirer(id, nomChasseurInconnu);

        chasseurInconnuVeutTirer.Should()
            .Throw<ChasseurInconnu>()
            .WithMessage($"Chasseur inconnu {nomChasseurInconnu}");

        repository.HasSavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerQuandPartieExistePas = () => service.Tirer(id, "Bernard");

        tirerQuandPartieExistePas.Should()
            .Throw<LaPartieDeChasseNexistePas>();

        repository.HasSavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstTerminée()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.Terminée,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

        tirerQuandTerminée.Should()
            .Throw<OnTirePasQuandLaPartieEstTerminée>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
    }

    [Fact]
    public void EchoueSiLesChasseursSontEnApero()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.Apéro,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

        tirerEnPleinApéro.Should()
            .Throw<OnTirePasPendantLapéroCestSacré>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
    }
}