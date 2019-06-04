### Ion CLI

The CLI compiler for the Ion language.

### Development Environment Notes

Please note that to setup a working development environment, the Ion git submodule must be initiated and updated using `git submodule init && git submodule update` and then built.


### Development environment setup

Developer env. setup is a breeze! Follow the steps below for your OS.

#### Windows

First, make sure you have Inno Setup installed, as this is used to package the Windows installer.

> [Click here to download it](http://www.jrsoftware.org/download.php/is.exe)

Now, simply `cd` into your desired development folder, and run the following one-liner:
```cmd
$ git clone https://github.com/IonLanguage/Ion.CLI && cd Ion.CLI/.scripts/windows && setup-env.bat
```

You're all set!

#### Additional notes

If you're developing the application, it is recommended you run it with the following command:

```shell
$ dotnet run -- --verbose --debug --root .test --tools-path .tools
```

### Installation

If you have downloaded this as a release, follow the instructions below to install the CLI utility locally on your machine:

#### Linux

Run the `install.sh` script:

```shell
$ bash install.sh
```

### Usage

Usage is simple. Once you've installed Ion on your platform, you can run the following command on a Windows Command Prompt (if you're on Windows) or a shell otherwise:

```shell
$ ion [build|run] [options]
```

### Options

```
-v, --verbose             Enable verbose mode, allowing verbose messages to be displayed.

-e, --exclude             Exclude certain directories from being processed.

-o, --output              (Default: bin) The output directory which the program will be emitted onto.

-r, --root                The root directory to start the scanning process from.

-b, --bitcode             Print out the LLVM Bitcode code instead of LLVM IR.

-s, --silent              Do not output any messages.

-i, --no-integrity        Skip integrity check.

-d, --debug               Use debugging mode.

-t, --tools-path          (Default: tools) Specify the tools directory path to use. Path is relative to the CLI's execution directory.

-k, --keep-emitted        Do not cleanup emitted files after compilation.

-x, --external-output     Whether to display external executables' output. Verbose mode must also be active.

-c, --ignore-exit-code    Whether to ignore the exit code of the program being run.

--help                    Display this help screen.

--version                 Display version information.

operation (pos. 0)        (Default: build) The operation to perform.
```
