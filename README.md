# RedPeanut
```
__________________________________________________________________________
ooooooo________________oo_ooooooo___________________________________oo____
oo____oo___ooooo___oooooo_oo____oo__ooooo___ooooo__oo_ooo__oo____o__oo____
oo____oo__oo____o_oo___oo_oo____oo_oo____o_oo___oo_ooo___o_oo____o_oooo___
ooooooo___ooooooo_oo___oo_oooooo___ooooooo_oo___oo_oo____o_oo____o__oo____
oo____oo__oo______oo___oo_oo_______oo______oo___oo_oo____o_ooo___o__oo__o_
oo_____oo__ooooo___oooooo_oo________ooooo___oooo_o_oo____o_oo_ooo____ooo__
__________________________________________________________________________
________________________________________________RedPeanut_v0.2.3___@b4rtik
__________________________________________________________________________
```
- - - -
Currently being tested.

Known modules Issues:

process -> spawnasagent

process -> spawnasshellcode
- - - -

RedPeanut is a small RAT developed in .Net Core 2 and its agent in .Net 3.5 / 4.0. RedPeanut 
code execution And on is based on shellcode generated with [Donut](https://github.com/TheWover/donut). It is therefore a hybrid, although developed in .Net it 
does not rely solely on the Assembly.Load. If you are interested in a .Net C2 Framework that is 
consistent and can be used in an enagement, I suggest [Covenant](https://github.com/cobbr/Covenant).

RedPeanut is weaponized with:

* GhostPack
* SharpGPOAbuse
* EvilClippy
* DotNetToJS
* SharpWeb
* Modified version of PsExec
* SharpSploit
* TikiTorch


## RedPeanut Agent

The RedPeanut agent can be compiled in .Net 3.5 and 4.0 and has pivoting capabilities via
NamedPipe. The agent performs its own critical tasks in a separate process to prevent the
AV response to detection or error during execution make you lose the whole
agent. The execution flow is as follows:

1. Process creation
2. Inject static shellcode generated with Donut
3. The loader retrieves the stager via namedpipe in GZip base64 encoded format (for all tasks except "lateralmsbuild")
4. The loader loads and executes the stager

For the "lateralmsbuild" task the loader retrieves the stager via the Windows Notification Facility
(this limits the scope of application).
The agent currently only supports https channel.


## C2 Channel

The agent checkin protocol is very simple:

1. The stager requires an agent id, the message is encrypted with RC4 with the shared serverkey
2. The server decrypts the message, compiles and sends the agent, generates and sends KEY and IV for future communications AES encryption, the message is encrypted RC4
3. The stager decrypts the message and loads the agent via Assembly.Load
4. The agent sends a checkin message to the server, the message is encrypted with AES

Alternatively, the covered channel feature can be activated(at the moment it is just a PoC). 
The idea is to imitate the web traffic carried out by a real user. Usually a web page is composed 
of the html page and all the objects necessary for its display as css, images, etc.
At the request of a new task the answer from the server will not be directly the encrypted 
task but an html page from which to extract the link to the image that will have emitted the 
encrypted task. The http request for the image will contain the Referer header.


## Content delivery

Content delivery is organized in 4 channels:

1. C2 Channe customizable via profile
2. Dynamic content generated/managed by RedPeanut customizable via profile
3. Static content mapped to /file/
3. Covered channel for the recovery of the image containing the payload mapped to /images/


## Profiles

RedPeanut capability of customization of network footprint both server side and client side.
The properties that can be set are:

* _General_
  * Delay (between requests)
  * ContentUri (url of dynamic content eg. dll hta etc.)
  * UserAgent
  * Spawn (the process to create to perform critical tasks)
  * HtmlCovered (Enable covered channel)
  * TargetClass (Class to search for image recover)  
* _Http Get_
  * ApiPath (comma separated list of url es /news-list.jsp,/antani.php etc.)
   * _Server_
     * Prepend
     * Append
     * Headers (name and value pair for http headers)
   * _Client_
     * Headers    
* _Http Post_
  * ApiPath (comma separated list of url es /news-list.jsp,/antani.php etc.)
  * Param (the name of the post request payload parameter)
  * Mask (format for interpreting the key value pair eg {0}={1}) !!!!!!!!!!
  * _Server_
    * Prepend
    * Append
    * Headers (name and value pair for http headers)
  * _Client_
    * Headers

_Domain Fronting_

To enable the domain fronting support it is necessary to value the "Host" header in the client section, both post and get (exemplified in the default profile 2)


## PowerShellExecuter

The PowerShellExecuter module allows you to execute oneliner commands or files in a runspace
with AMSI bypass, Logging bypass and PowerView already loaded.


## Launchers

* Exe
* Dll
* PowerShell
* Hta (vbs,powershell)
* InstallUtil
* MSBuild
* MacroVba


## Local modules

* EvilClippy


## Agent Tasks

* Upload
* DownLoad
* SharpWeb
* SharpWmi
* SharpUp
* UACBypass Token Duplication
* SharpDPAPIVaults
* SharpDPAPITriage
* SharpDPAPIRdg
* SharpDPAPIMasterKeys
* SharpDPAPIMachineVaults
* SharpDPAPIMachineTriage
* SharpDPAPIMachineMasterKeys
* SharpDPAPIMachineCredentials
* SharpDPAPICredentials
* SharpDPAPIBackupKey
* Seatbelt
* SafetyKatz
* RubeusTriage
* RubeusTgtDeleg
* RubeusS4U
* RubeusRenew
* RubeusPurge
* RubeusPtt
* RubeusMonitor
* RubeusKlist
* RubeusKerberoast
* RubeusHash
* RubeusHarvest
* RubeusDump
* RubeusDescribe
* RubeusCreateNetOnly
* RubeusChangePw
* RubeusASREPRoast
* RubeusAskTgt
* SharpGPOAddUserRights
* SharpGPOAddStartupScript
* SharpGPOAddLocalAdmin
* SharpGPOAddImmediateTask
* PowerShellExecuter
* LatteralMSBuild
* SharpPsExec
* SharpAdidnsdump
* PPIDAgent
* SpawnAsAgent
* SpawnShellcode
* SpawnAsShellcode


## Persistence

* Autorun
* Startup
* WMI



## Running

To run RedPeanut you need to have dotnet installed. To install dotnet on Kali:

```
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
wget -q https://packages.microsoft.com/config/debian/9/prod.list
mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
chown root:root /etc/apt/sources.list.d/microsoft-prod.list

apt-get install apt-transport-https
apt-get update
apt-get install dotnet-sdk-2.1
```

```
git clone --recursive https://github.com/b4rtik/RedPeanut.git
```

For the covered channel functionality it is necessary to install the libgdiplus library, 
therefore:

For linux users:

```
apt-get install -y libgdiplus
```

For OSx

```
brew install mono-libgdiplus
```

```
root@kali:~# cd RedPanut
root@kali:~/RedPeanut# dotnet run
Using launch settings from /root/Projects/RedPeanut/Properties/launchSettings.json...
Enter password to encrypt serverkey: 

__________________________________________________________________________
ooooooo________________oo_ooooooo___________________________________oo____
oo____oo___ooooo___oooooo_oo____oo__ooooo___ooooo__oo_ooo__oo____o__oo____
oo____oo__oo____o_oo___oo_oo____oo_oo____o_oo___oo_ooo___o_oo____o_oooo___
ooooooo___ooooooo_oo___oo_oooooo___ooooooo_oo___oo_oo____o_oo____o__oo____
oo____oo__oo______oo___oo_oo_______oo______oo___oo_oo____o_ooo___o__oo__o_
oo_____oo__ooooo___oooooo_oo________ooooo___oooo_o_oo____o_oo_ooo____ooo__
__________________________________________________________________________
________________________________________________RedPeanut_v0.2.3___@b4rtik
__________________________________________________________________________

[*] No profile avalilable, creating new one...
[RP] >
```

## Donut static Shellcode to dynamic assembly execution 

Donut is a shellcode generation tool that creates position-independant shellcode payloads from .NET Assemblies. 
This shellcode may be used to inject the Assembly into arbitrary Windows processes. 
Given an arbitrary .NET Assembly, parameters, and an entry point (such as Program.Main), 
it produces position-independent shellcode that loads it from memory. 
The .NET Assembly can either be staged from a URL or stageless by being embedded directly in the shellcode. 

Donut produces a shellcode with the embedded target assembly. This introduces a certain rigidity. While waiting for a shellcode generator to be used with .Net Core 2, I have created two possible solutions in this [repository](https://github.com/b4rtik/DonutSupport) :
1. Recover the assembly from the calling process via NamedPipe [InjectionLoader](https://github.com/b4rtik/DonutSupport)
2. Retrieve the assembly from the calling process using Wnf [InjectionLoaderWnf](https://github.com/b4rtik/DonutSupport)


```
PS Z:\donut> .\donut.exe -donut -f Z:\DonutSupport\InjectionLoaderWnf\bin\x64\Release\InjectionLoaderWnf.dll -a 2 -c InjectionLoaderWnf -m LoadRP

  [ Donut .NET shellcode generator v0.9
  [ Copyright (c) 2019 TheWover, Odzhan

  [ Instance Type : PIC
  [ .NET Assembly : "Z:\DonutSupport\InjectionLoaderWnf\bin\x64\Release\InjectionLoaderWnf.dll"
  [ Assembly Type : DLL
  [ Class         : InjectionLoaderWnf
  [ Method        : LoadRP
  [ Target CPU    : AMD64
  [ Shellcode     : "payload.bin"

PS Z:\donut>  Get-CompressedShellcode Z:\donut\payload.bin Z:\RedPeanut\Resources\nutclrwnf.txt
PS Z:\donut> .\donut.exe -donut -f Z:\DonutSupport\InjectionLoader\bin\x64\Release\InjectionLoader.dll -a 2 -c InjectionLoader -m LoadRP

  [ Donut .NET shellcode generator v0.9
  [ Copyright (c) 2019 TheWover, Odzhan

  [ Instance Type : PIC
  [ .NET Assembly : "Z:\DonutSupport\InjectionLoader\bin\x64\Release\InjectionLoader.dll"
  [ Assembly Type : DLL
  [ Class         : InjectionLoader
  [ Method        : LoadRP
  [ Target CPU    : AMD64
  [ Shellcode     : "payload.bin"

PS Z:\donut> Get-CompressedShellcode Z:\donut\payload.bin Z:\RedPeanut\Resources\nutclr.txt
PS Z:\donut>
```

## Tools updating

Some of the well-known tools present in RedPeanut such as the GhostPack tools are wrapped 
in full and executed on the client side. To update the tools, for example SeatBelt, without 
updating the entire repository is necessary: Clone the Seatbelt repository, rename the "Main" 
method in "Execute", insert the public modifier and recompile as dll. The dll must be compressed 
and encoded in Base64 with the ps RastaMouse script [Get-CompressedShellcode.ps1](https://github.com/rasta-mouse/TikiTorch/blob/dev/Get-CompressedShellcode.ps1)

## Credits

* Donut - [@TheRealWover](https://twitter.com/TheRealWover)
* SharpSploit - [@cobbr_io](https://twitter.com/cobbr_io)
* GhostPack - [@harmj0y](https://twitter.com/harmj0y)
* SharpGPOAbuse - [@mwrlabs](https://twitter.com/mwrlabs)
* EvilClippy - [@StanHacked](https://twitter.com/StanHacked)
* DotNetToJS - [@tiraniddo](https://twitter.com/tiraniddo)
* Sharpshooter - [@domchell](https://twitter.com/domchell)
* SharpWeb - [@djhohnstein](https://twitter.com/djhohnstein)
* Original version of PsExec - [@malcomvetter](https://twitter.com/malcomvetter)
* TikiTorch - [@_RastaMouse](https://twitter.com/_RastaMouse)


