# CodingFS

Filter files by its type (source, dependency, generated). The type is detected by coding workspaces.

All supported workspaces: [CodingFS/Workspaces](CodingFS/Workspaces).

# Cli

When running CodingFS from the command line, CodingFS will load config file from `config.toml`. You can also explicitly specify a config file to use with the --config CLI option.

Config file example:

```toml
# The starting directory to traverse, used in Mount and List command.
Root = "D:\\Coding"

# Maximum search depth (include the root).
MaxDepth = 6

# Additional paths are considered to be dependency.
Deps = []

# Additional paths are considered to be ignored.
Ingores = [
	"D:\\Coding\\ThirdParty",
	"D:\\Coding\\temp",
]

[Detector]
# Set the type of files matched by .gitignore, NotCare, Dependency, or Ignored.
Gitignore = "NotCare"
```

## Mount

```
CodingFS mount <MountPoint> [--config=<config.toml>] [--type=<Source>] [--label=<SourceView>] [--readonly=<bool>]
```

Map a directory to a virtual drive, containing only files of the specified type. You must install [Dokan](https://github.com/dokan-dev/dokany) driver to run this command.

* `MountPoint`: drive letter of the mouted filesystem. Required.
* `--config`: Path of the config file to use.
* `--type`: Which type of files should be included in the file system. Avaliable values: Source, Dependency, Generated. Use comma to separate flags.
* `--label`: Volume label on Windows.
* `--readonly`: Mount the volume as read-only. Default true.

CodingFS does not watch files, changes made after mounting may not be reflected on the virtual volume.

> [!WARNING]
> If you set `--type` without `Source`, CodingFS need to scan the directory recursively before mounting, This may take a while, you can limit the depth by set `MaxDepth` property in the config file.

## List

```
CodingFS list [--config=<config.toml>] [--type=<Source>] [--name-only]
```

List files in root directory of the specific type.

* `--config`: Path of the config file to use.
* `--type`: Which type of files should be listed. Avaliable values: Source, Dependency, Generated. Use comma to separate flags.
* `--name-only`: Only show file names.

## Inspect

```
CodingFS inspect [D:\path\to\inspect]
```

Show workspaces and their recognize result for the specific path. Useful for debug.
