# XPath2.Net : Lightweight XPath2 for .NET

[![Build status](https://ci.appveyor.com/api/projects/status/kwow5frjv8nl42oy?svg=true)](https://ci.appveyor.com/project/StefH/xpath2-net)
[![codecov](https://codecov.io/gh/StefH/XPath2.Net/branch/master/graph/badge.svg)](https://codecov.io/gh/StefH/XPath2.Net)
[![Coverage Status](https://coveralls.io/repos/github/StefH/XPath2.Net/badge.svg)](https://coveralls.io/github/StefH/XPath2.Net)

| Project | NuGet |
| ------- | ----- |
| XPath2  | [![NuGet Badge](https://buildstats.info/nuget/XPath2)](https://www.nuget.org/packages/XPath2) |
| XPath2.Extensions | [![NuGet Badge](https://buildstats.info/nuget/XPath2.Extensions)](https://www.nuget.org/packages/XPath2.Extensions) |

Project is forked from https://xpath2.codeplex.com/

## Supported frameworks: ##
- .NET 3.5
- .NET 4.0 and up
- .NET Standard 2.0

This is an implementation of W3C XML Path Language (XPath) 2.0 for .NET Framework based on standard XPathNavigator API.
The given implementation based on practice of developing XQuery is fully corresponding to the specification demands. This is single assembly 302K size.

It conforms for 12952 from 15133 (85.59%) regarding the test-set (XQTS 1.0.2 Nov. 20, 2006) at https://dev.w3.org/2006/xquery-test-suite/PublicPagesStagingArea/

API used is an anology to the standard one built into the platform: you utilize
XPath2Expression instead of the common XPathExpression and a set of extension
functions for XNode, XPathNavigator and XmlNode classes.

| System.Xml.XPath 	          | WmHelp.XPath2
| ----------------------------|------------------------------
| XPathNavigator.Evaluate()   | XPathNavigator.XPath2Evaluate()
| XmlNode.SelectNodes() 	  | XmlNode.XPath2SelectNodes()
| XmlNode.SelectSingleNode()  | XmlNode.XPath2SelectSingleNode()
| XNode.Select<T>() 	      | XNode.XPath2Select<T>()
| ..., etc.                   | ..., etc.


In addition there parameterized XPath2 expressions are
implemented that allow to compose XQuery requests by standard Linq-to-XML means.
Though it is not generally necessary to use XPath during writing Linq-to-XML queries
nevertheless they allow to make it simple.
To pass variable values into XPath expression we use C# 4.0 anonimous
structures, e.g. new { varname = value, ... }
creates variable $varname,... inside XPath expression.

Here're examples of some W3C XQuery usecases and their translations
into C# and LINQ.

**XQuery (RQ2.xq)**
```xml
<result>
  {
    for $i in doc("items.xml")//item_tuple
    let $b := doc("bids.xml")//bid_tuple[itemno = $i/itemno]
    where contains($i/description, "Bicycle")
    order by $i/itemno
    return
        <item_tuple>
            { $i/itemno }
            { $i/description }
            <high_bid>{ max($b/bid) }</high_bid>
        </item_tuple>
  }
</result> 
```
**C#**
```c#
XNode items = XDocument.Load("items.xml");
XNode bids = XDocument.Load("bids.xml");
XNode result = new XDocument(
    new XElement("result",
        (from item in items.XPath2SelectElements("//item_tuple")
                .OrderBy((elem) => (string)elem.Element("itemno"))                       
            let bid = bids.XPath2Select<XElement>("//bid_tuple[itemno = $i/itemno]", new { i = item })
            where ((string)item.Element("description")).Contains("Bicycle")
            select new XElement("item_tuple", 
                item.Element("itemno"),
                item.Element("description"),
                !bid.Any() ?  null :
                    new XElement("high_bid", 
                        bid.AsQueryable().Max((elem) => (double)elem.Element("bid"))))))
);
```

**XQuery (RQ3.xq)**
```xml
<result>
  {
    for $u in doc("users.xml")//user_tuple
    for $i in doc("items.xml")//item_tuple
    where $u/rating > "C" 
       and $i/reserve_price > 1000 
       and $i/offered_by = $u/userid
    return
        <warning>
            { $u/name }
            { $u/rating }
            { $i/description }
            { $i/reserve_price }
        </warning>
  }
</result>
```
**C#**
```c#
XNode users = XDocument.Load("users.xml");
XNode items = XDocument.Load("items.xml");
XNode result = 
    new XElement("result",
        (from user in users.XPath2SelectElements("//user_tuple")
            from item in items.XPath2SelectElements("//item_tuple")
            where (bool)XPath2Expression.Evaluate(@"$u/rating > 'C' and $i/reserve_price > 1000 
                    and $i/offered_by = $u/userid", new { u = user, i = item })
            select new XElement("warning",
                user.Element("name"),
                user.Element("rating"),
                item.Element("description"),
                item.Element("reserve_price"))));

```

**XQuery (RQ9.xq)**
```xml
<result>
  {
    let $end_dates := doc("items.xml")//item_tuple/end_date
    for $m in distinct-values(for $e in $end_dates 
                              return month-from-date($e))
    let $item := doc("items.xml")
        //item_tuple[year-from-date(end_date) = 1999 
                     and month-from-date(end_date) = $m]
    order by $m
    return
        <monthly_result>
            <month>{ $m }</month>
            <item_count>{ count($item) }</item_count>
        </monthly_result>
  }
</result>
```
**C#**
```c#
XNode items = XDocument.Load("items.xml");
var endDates = items.XPath2SelectElements("//item_tuple/end_date");
XNode result = 
    new XElement("result",
        (from month in XPath2Expression.SelectValues(@"distinct-values(for $e in $end_dates 
                    return month-from-date($e))", new { end_dates = endDates }).OrderBy((arg) => arg)
            let item = items.XPath2SelectElements(@"//item_tuple[year-from-date(end_date) = 1999 
                    and month-from-date(end_date) = $m]", new { m = month })
            select new XElement("monthly_result",
                new XElement("month", month),
                new XElement("item_count", item.Count())
            )));
```
