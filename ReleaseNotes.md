# 1.1.4 (29 November 2023)
- [#60](https://github.com/StefH/XPath2.Net/pull/60) - Add extra unit-tests for issue 59 + implement some Nullable fixes to the code [enhancement] contributed by [StefH](https://github.com/StefH)
- [#61](https://github.com/StefH/XPath2.Net/pull/61) - Add more unit tests for issue 58 [enhancement] contributed by [StefH](https://github.com/StefH)
- [#64](https://github.com/StefH/XPath2.Net/pull/64) - handle &amp;nbsp; in NormalizeStringValue() [bug] contributed by [jgilbert2017](https://github.com/jgilbert2017)
- [#66](https://github.com/StefH/XPath2.Net/pull/66) - Add unit tests for CoreFuncs.NormalizeStringValue [tests] contributed by [StefH](https://github.com/StefH)
- [#63](https://github.com/StefH/XPath2.Net/issues/63) - Missing parsing for some html-encoded entity references [bug]

# 1.1.3 (25 April 2022)
- [#49](https://github.com/StefH/XPath2.Net/pull/49) - Fixed rules in XPath.y for DocumentTest and added test cases that sho&#8230; contributed by [martin-honnen](https://github.com/martin-honnen)
- [#50](https://github.com/StefH/XPath2.Net/pull/50) - Add XPath2.TestRunner Project + ConsoleApp [enhancement] contributed by [StefH](https://github.com/StefH)
- [#51](https://github.com/StefH/XPath2.Net/pull/51) - Add AttributeSelection tests [tests] contributed by [StefH](https://github.com/StefH)
- [#52](https://github.com/StefH/XPath2.Net/pull/52) - Fix TestRunner contributed by [StefH](https://github.com/StefH)
- [#53](https://github.com/StefH/XPath2.Net/pull/53) - Update TestRunner (fix for implicit-timezone) contributed by [StefH](https://github.com/StefH)
- [#54](https://github.com/StefH/XPath2.Net/pull/54) - Runner + unittests [tests] contributed by [StefH](https://github.com/StefH)
- [#55](https://github.com/StefH/XPath2.Net/pull/55) - Fix some SonarCloud issues [enhancement] contributed by [StefH](https://github.com/StefH)
- [#56](https://github.com/StefH/XPath2.Net/pull/56) - [Snyk] Security upgrade Newtonsoft.Json from 11.0.2 to 13.0.1 contributed by [StefH](https://github.com/StefH)
- [#57](https://github.com/StefH/XPath2.Net/pull/57) - Update NuGet packages [enhancement] contributed by [StefH](https://github.com/StefH)
- [#47](https://github.com/StefH/XPath2.Net/issues/47) - Attribute node selection with &quot;kind test&quot; attribute() or attribute(foo) doesn't work
- [#48](https://github.com/StefH/XPath2.Net/issues/48) - Node test of the form document-node(element(foo)) seems broken [bug]

# 1.1.2 (23 June 2021)
- [#45](https://github.com/StefH/XPath2.Net/pull/45) - XPath2Evaluate - ToString [bug] contributed by [StefH](https://github.com/StefH)
- [#13](https://github.com/StefH/XPath2.Net/issues/13) - Using XPath2Evaluate() without a function in command there's always a comma at the end of the value [bug]

# 1.1.1 (18 June 2021)
- [#42](https://github.com/StefH/XPath2.Net/pull/42) - Default element namespace fix contributed by [martin-honnen](https://github.com/martin-honnen)
- [#43](https://github.com/StefH/XPath2.Net/pull/43) - Fix CI build [enhancement] contributed by [StefH](https://github.com/StefH)
- [#31](https://github.com/StefH/XPath2.Net/issues/31) - Default namespace not handled in XPath query [enhancement]

# 1.1.0.0 (08 September 2020)
- [#36](https://github.com/StefH/XPath2.Net/pull/36) - Rewrite yyerror method to fix SonarQube issue [enhancement] contributed by [StefH](https://github.com/StefH)
- [#37](https://github.com/StefH/XPath2.Net/pull/37) - Make ast public [enhancement] contributed by [binarycow](https://github.com/binarycow)
- [#35](https://github.com/StefH/XPath2.Net/issues/35) - Expose the AST? [enhancement]

# 1.0.12.0 (04 August 2020)
- [#34](https://github.com/StefH/XPath2.Net/pull/34) - Fixed 'XPath2Expression.Compile throws exception in Mono' [bug] contributed by [StefH](https://github.com/StefH)
- [#33](https://github.com/StefH/XPath2.Net/issues/33) - XPath2Expression.Compile throws exception in Mono [bug]

# 1.0.11.0 (08 May 2020)
- [#30](https://github.com/StefH/XPath2.Net/pull/30) - Fix NuGet for net40 and add source-link [bug] contributed by [StefH](https://github.com/StefH)
- [#29](https://github.com/StefH/XPath2.Net/issues/29) - Wrong version of XPath.dll is packaged in the 1.0.10 nupkg for net40 [bug]

# 1.0.10.0 (11 September 2019)
- [#28](https://github.com/StefH/XPath2.Net/pull/28) - Fix &quot;MoveToNext&quot; (throws Null Exception on .NET Standard / Core) [bug] contributed by [StefH](https://github.com/StefH)
- [#27](https://github.com/StefH/XPath2.Net/issues/27) - Selecting multiple results throws null exception on .NET Core [bug]

# 1.0.9.0 (18 June 2019)
- [#26](https://github.com/StefH/XPath2.Net/pull/26) - Fix Round() [bug] contributed by [StefH](https://github.com/StefH)
- [#25](https://github.com/StefH/XPath2.Net/issues/25) - round(2.5) evaluates to 2 [bug]

# 1.0.8.0 (28 May 2019)
- [#24](https://github.com/StefH/XPath2.Net/pull/24) - Add function &quot;string()&quot; [bug] contributed by [StefH](https://github.com/StefH)
- [#23](https://github.com/StefH/XPath2.Net/issues/23) - Calling string() in step fails with Wmhelp.XPath2.XPath2Exception: The function 'string'/0 was not found [bug]

# 1.0.7.0 (23 May 2019)
- [#22](https://github.com/StefH/XPath2.Net/pull/22) - Using a variable in XPath2Select on an XPathNavigator should work [bug] contributed by [StefH](https://github.com/StefH)
- [#20](https://github.com/StefH/XPath2.Net/issues/20) - Should using  a variable in XPath2Select on an XPathNavigator work?

# 1.0.6.1 (29 October 2018)
- [#16](https://github.com/StefH/XPath2.Net/pull/16) - ns:number function does not operate on a node #15 contributed by [wjgerritsen-0001](https://github.com/wjgerritsen-0001)
- [#18](https://github.com/StefH/XPath2.Net/pull/18) - Set up CI with Azure Pipelines contributed by [azure-pipelines[bot]](https://github.com/apps/azure-pipelines)
- [#15](https://github.com/StefH/XPath2.Net/issues/15) - ns:number function does not operate on a node
- [#17](https://github.com/StefH/XPath2.Net/issues/17) - Implement Azure Pipelines for CI (remove app-veyor) [enhancement]

# 1.0.6.0 (29 October 2018)
- [#19](https://github.com/StefH/XPath2.Net/pull/19) - sign + fix appveyor contributed by [StefH](https://github.com/StefH)
- [#10](https://github.com/StefH/XPath2.Net/issues/10) - Bug in netstandard 2.0 : XPath2Exception in simple Compile [bug]

# 1.0.5.1 (15 August 2017)
- [#8](https://github.com/StefH/XPath2.Net/pull/8) - Added netstandard2.0 target framework [enhancement] contributed by [kashifsoofi](https://github.com/kashifsoofi)
- [#11](https://github.com/StefH/XPath2.Net/pull/11) - Netstandard20 fix contributed by [StefH](https://github.com/StefH)
- [#1](https://github.com/StefH/XPath2.Net/issues/1) - FunctionTable.Instance.AddAllExtensions throws exception when called multiple times [bug]
- [#2](https://github.com/StefH/XPath2.Net/issues/2) - Fix version from XPath2 to 1.0.2 [enhancement]
- [#3](https://github.com/StefH/XPath2.Net/issues/3) - Update project to the NETStandard 1.6
- [#4](https://github.com/StefH/XPath2.Net/issues/4) - spelling mistake
- [#5](https://github.com/StefH/XPath2.Net/issues/5) - Add json-to-xml / json-to-xmlstring extensions [enhancement]

