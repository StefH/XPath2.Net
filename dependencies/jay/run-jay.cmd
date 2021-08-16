jay -cv "Xpath.y" < "skeleton.cs" > "../../src/XPath2/XPath.cs"
del y.output

rem Copy the Xpath.y file to the src-folder to enable debugger
copy "Xpath.y" "../../src/XPath2/Xpath.y" /Y