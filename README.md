Step1: Compile from source
SET DOTNET_PATH=c:\Windows\Microsoft.NET\Framework\v4.0.30319\
%DOTNET_PATH%csc.exe /target:winexe /optimize /out:browserselect.exe /win32icon:icon.ico *.cs

Step2: Copy the new executable to the desired destination
Step3: Edit the install.reg to reflect the new destination, then add it into the registry.

