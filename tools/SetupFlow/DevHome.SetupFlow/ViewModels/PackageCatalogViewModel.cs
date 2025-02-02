﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Models;
using DevHome.SetupFlow.Services;

namespace DevHome.SetupFlow.ViewModels;

/// <summary>
/// Delegate factory for creating package catalog view models
/// </summary>
/// <param name="catalog">Package catalog</param>
/// <returns>Package catalog view model</returns>
public delegate PackageCatalogViewModel PackageCatalogViewModelFactory(PackageCatalog catalog);

/// <summary>
/// ViewModel class for a <see cref="PackageCatalog"/> model.
/// </summary>
public partial class PackageCatalogViewModel : ObservableObject
{
    private readonly IScreenReaderService _screenReaderService;
    private readonly ISetupFlowStringResource _stringResource;
    private readonly PackageCatalog _packageCatalog;

    [ObservableProperty]
    private bool _canAddAllPackages;

    public string Name => _packageCatalog.Name;

    public string Description => _packageCatalog.Description;

    public IReadOnlyCollection<PackageViewModel> Packages { get; private set; }

    public PackageCatalogViewModel(
        PackageProvider packageProvider,
        PackageCatalog packageCatalog,
        IScreenReaderService screenReaderService,
        ISetupFlowStringResource stringResource)
    {
        _packageCatalog = packageCatalog;
        _screenReaderService = screenReaderService;
        _stringResource = stringResource;
        Packages = packageCatalog.Packages
            .Select(p => packageProvider.CreateOrGet(p, cachePermanently: true))
            .OrderBy(p => p.IsInstalled)
            .ToReadOnlyCollection();
    }

    [RelayCommand]
    private void AddAllPackages()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Adding all packages from catalog {Name} to selection");
        foreach (var package in Packages)
        {
            // Select all non-installed packages
            if (!package.IsInstalled)
            {
                package.IsSelected = true;
            }
        }

        // TODO Explore option to augment a Button with the option to announce a text when invoked.
        // https://github.com/microsoft/devhome/issues/1451
        _screenReaderService.Announce(_stringResource.GetLocalized(StringResourceKey.AddAllApplications));
    }
}
