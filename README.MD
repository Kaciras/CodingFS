# CodingFS

Mount a directory to virtual volum, contains only specific type of files (source, dependency, generated). 

The type is detected by coding workspaces, supports:

- Intellij IDEA
- VSCode
- VisualStudio
- Git
- Npm
- Cargo
- NuGet

Not all configuration items, and plugins are supported, this is an endless task. If CodingFS doesn't recognize files in your workspace, feel free to submit issues and PRs.

Requirement: .NET >= 8

![Three type of files](https://github.com/Kaciras/CodingFS/raw/master/MountViews.webp)

## GUI

`CodingFS.GUI.exe` is a GUI for mounting CodingFS virtual drive.

![Three type of files](https://github.com/Kaciras/CodingFS/raw/master/Screenshot.webp)

## CLI

CodingFS cli need a config file to run, by default is `config.toml`, it's also the default file used in GUI.

You can also explicitly specify a config file to use with the `--config` CLI option.

[Default config file](https://github.com/Kaciras/CodingFS/blob/master/Cli/config.toml)

### Mount

```
CodingFS mount <MountPoint> [--config=<config.toml>] [--type=<Source>] [--label=<SourceView>] [--readonly=<bool>]
```

Map the directory to a virtual drive, containing only files of the specified type. You must install [Dokan](https://github.com/dokan-dev/dokany) driver to run this command.

* `MountPoint`: drive letter of the mouted filesystem. Required.
* `--config`: Path of the config file to use.
* `--type`: Which type of files should be included in the file system. Avaliable values: Source, Dependency, Generated. Use comma to separate flags.
* `--label`: Volume label on Windows.
* `--readonly`: Mount the volume as read-only. Default false.

CodingFS does not watch files, changes made after mounting may not be reflected on the virtual volume.

> [!WARNING]
> If you set `--type` without `Source`, CodingFS need to scan the directory recursively before mounting, This may take a while, you can limit the depth by set `MaxDepth` property in the config file.

### List

```
CodingFS list [--config=<config.toml>] [--type=<Source>] [--name-only]
```

List files in root directory of the specific type.

* `--config`: Path of the config file to use.
* `--type`: Which type of files should be listed. Avaliable values: Source, Dependency, Generated. Use comma to separate flags.
* `--name-only`: Only show file names.

### Inspect

```
CodingFS inspect [D:\path\to\inspect]
```

Show workspaces and their recognize result for the specific path. Useful for debug.
