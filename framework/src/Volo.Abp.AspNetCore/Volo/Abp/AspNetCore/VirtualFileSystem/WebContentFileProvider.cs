﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Volo.Abp.DependencyInjection;
using Volo.Abp.VirtualFileSystem;

namespace Volo.Abp.AspNetCore.VirtualFileSystem;

public class WebContentFileProvider : IWebContentFileProvider, ISingletonDependency
{
    private readonly IVirtualFileProvider _virtualFileProvider;
    private readonly IFileProvider _fileProvider;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private string _rootPath = "/wwwroot";

    protected AbpAspNetCoreContentOptions Options { get; }

    public WebContentFileProvider(
        IVirtualFileProvider virtualFileProvider,
        IWebHostEnvironment hostingEnvironment,
        IOptions<AbpAspNetCoreContentOptions> options)
    {
        _virtualFileProvider = virtualFileProvider;
        _hostingEnvironment = hostingEnvironment;
        Options = options.Value;

        _fileProvider = CreateFileProvider();
    }

    public virtual IFileInfo GetFileInfo(string subpath)
    {
        if (string.IsNullOrEmpty(subpath))
        {
            return new NotFoundFileInfo(subpath);
        }

        if (PathUtils.PathNavigatesAboveRoot(subpath))
        {
            return new NotFoundFileInfo(subpath);
        }

        if (ExtraAllowedFolder(subpath) && ExtraAllowedExtension(subpath))
        {
            var fileInfo = _fileProvider.GetFileInfo(subpath);
            if (fileInfo.Exists)
            {
                return fileInfo;
            }
        }

        return _fileProvider.GetFileInfo(_rootPath + subpath);
    }

    public virtual IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (subpath == null || PathUtils.PathNavigatesAboveRoot(subpath))
        {
            return NotFoundDirectoryContents.Singleton;
        }

        if (ExtraAllowedFolder(subpath))
        {
            var directory = _fileProvider.GetDirectoryContents(subpath);
            if (directory.Exists)
            {
                return directory;
            }
        }

        return _fileProvider.GetDirectoryContents(_rootPath + subpath);
    }

    public virtual IChangeToken Watch(string filter)
    {
        if (!ExtraAllowedFolder(filter))
        {
            return _fileProvider.Watch(_rootPath + filter);
        }

        return new CompositeChangeToken(
            new[]
            {
                    _fileProvider.Watch(_rootPath + filter),
                    _fileProvider.Watch(filter)
            }
        );
    }

    protected virtual IFileProvider CreateFileProvider()
    {
        var fileProviders = new List<IFileProvider>
            {
                new PhysicalFileProvider(_hostingEnvironment.ContentRootPath),
                _virtualFileProvider
            };

        return new CompositeFileProvider(
            fileProviders
        );
    }

    protected virtual bool ExtraAllowedFolder(string path)
    {
        return Options.AllowedExtraWebContentFolders.Any(s => path.StartsWith(s, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual bool ExtraAllowedExtension(string path)
    {
        return Options.AllowedExtraWebContentFileExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase));
    }
}
