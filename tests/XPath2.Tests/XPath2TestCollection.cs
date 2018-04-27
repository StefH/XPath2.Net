﻿using Xunit;

namespace XPath2.Tests
{
    [CollectionDefinition("xpath2 collection")]
    public class XPath2TestCollection : ICollectionFixture<XPath2TestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}