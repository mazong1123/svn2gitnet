"C:\Program Files (x86)\WiX Toolset v3.11\bin\heat.exe" dir ".\src\bin\Release\netcoreapp3.1\win10-x64\publish" -cg svn2gitnet -gg -sfrag -sreg -svb6 -template product -t svn2gitnet.xslt -out svn2gitnet-x64.wxs
"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" svn2gitnet-x64.wxs
"C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" -ext WixUIExtension -cultures:en-us -b ".\src\bin\Release\netcoreapp3.1\win10-x64\publish" .\svn2gitnet-x64.wixobj

"C:\Program Files (x86)\WiX Toolset v3.11\bin\heat.exe" dir ".\src\bin\Release\netcoreapp3.1\win10-x86\publish" -cg svn2gitnet -gg -sfrag -sreg -svb6 -template product -t svn2gitnet.xslt -out svn2gitnet-x86.wxs
"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" svn2gitnet-x86.wxs
"C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" -ext WixUIExtension -cultures:en-us -b ".\src\bin\Release\netcoreapp3.1\win10-x86\publish" .\svn2gitnet-x86.wixobj