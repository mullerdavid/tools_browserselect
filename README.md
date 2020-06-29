Step1: Compile from source
SET DOTNET_PATH=c:\Windows\Microsoft.NET\Framework\v4.0.30319\
%DOTNET_PATH%csc.exe /target:winexe /optimize /out:browserselect.exe /win32icon:icon.ico /resource:urlscan.png /resource:virustotal.png *.cs

Step2: Copy the new executable and the config.xml to the desired destination
Step3: Edit the install_win10.reg (or win7) to reflect the new destination, then add it into the registry.
Step4: Edit the config.xml with your API keys and PATH for URLScan and Virustotal. (PATH is the browser you want to open the results)
