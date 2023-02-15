# Eris
## DISCLAIMER
This program is use for education purposes only, and it should not be use beyond what is supposed to, without a mutual permission. The author is not responsible or not held liable to any damage or misuse that was made by using this program.

USE AT YOUR OWN RISK.
</br>

## Description
Eris is a .NET-based file padding program used to manipulate file's size by enlarging with random garbage data, or padding, without affecting its functionality.

This file padding method is known in [MITRE ATT&CK](https://attack.mitre.org/techniques/T1027/001/), as a part of file obfuscation:
<blockquote>
Adversaries may use binary padding to add junk data and change the on-disk representation of malware. This can be done without affecting the functionality or behavior of a binary, but can increase the size of the binary beyond what some security tools are capable of handling due to file size limitations.
</blockquote>
</br>

## Usage

```cmd
dotnet eris.dll -path <path> [options]
```
Argument:
<blockquote>
-path &lt;path&gt;: A path of a file to be enlarge/padded.
</blockquote>
</br>
Options:
<blockquote>

-sizeGB &lt;size&gt;: Adjust the size of a padded file in gigabytes (GB)

-sizeMB &lt;size&gt;: Adjust the size of a padded file in megabytes (MB)

(If the option -sizeMB or -sizeGB was not specified, A default padded size of 100 MB will be set.)

---
-n: Null-based padding bytes.

-z: Zero-based padding bytes.

-r: Randomized-based padding bytes.

(A default padding byte character is null-based padding type. ('-n'))
</blockquote>

---
example (where FileToPad.exe's size will enlarge with 2GB of padded bytes.):
```cmd
dotnet eris.dll -path C:\FileToPad.exe -sizeGB 2
```
</br>

## Compiling from Source
To compile this program, make sure your computer has .NET 7 SDK or a Visual Studio 2022 installed.

If you haven't a project already, download this project's ZIP file on GitHub, or clone it using 'git clone':
```cmd
git clone https://github.com/PheeLeep/Eris
```
---
### Compile in Visual Studio
Go to the folder where the project's ZIP file resides when you download, extract its contents, and open "Eris.vbproj" in Visual Studio.

Then click "Build" > "Build Solution". If there are no errors occurred, go to its bin folder and go to Debug/Release (depending on configuration you set)

---
### Compile in Command Line
Go to the folder where the project's ZIP file resides when you download, extract its contents, right-click in empty area and click "Open in Terminal"

Type this command to build:
```cmd
dotnet build Eris.vbproj
```
If there are no errors occurred, go to its bin folder and go to Debug/Release (depending on configuration you set)

## License
[MIT License](https://github.com/PheeLeep/Eris/blob/master/LICENSE.txt)