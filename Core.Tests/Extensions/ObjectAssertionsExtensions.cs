using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;

namespace Core.Tests.Extensions;

public static class ObjectAssertionsExtensions
{
    public static AndConstraint<TAssertions> BeEquivalentToIgnoreCycles<TSubject, TAssertions,TExpectation>(this ObjectAssertions<TSubject, TAssertions> parent, TExpectation expectation) where TAssertions : ObjectAssertions<TSubject, TAssertions>
    {
        return parent.BeEquivalentTo(expectation, opt => opt
            .IgnoringCyclicReferences()
            .WithoutStrictOrdering());
    }
    
    public static AndConstraint<TAssertions> BeEquivalentToIgnoreCycles<TCollection, TSource,TAssertions,TExpectation>(this GenericCollectionAssertions<TCollection, TSource,TAssertions> parent, IEnumerable<TExpectation> expectation) where TCollection : IEnumerable<TSource> where TAssertions : GenericCollectionAssertions<TCollection, TSource, TAssertions>
    {
        return parent.BeEquivalentTo(expectation, opt => opt
            .IgnoringCyclicReferences()
            .WithoutStrictOrdering());
    }
    
    public static AndConstraint<TAssertions> ContainEquivalentOfIgnoreCycles<TCollection, TSource,TAssertions,TExpectation>(this GenericCollectionAssertions<TCollection, TSource,TAssertions> parent, TExpectation expectation) where TCollection : IEnumerable<TSource> where TAssertions : GenericCollectionAssertions<TCollection, TSource, TAssertions>
    {
        return parent.ContainEquivalentOf(expectation, opt => opt
            .IgnoringCyclicReferences()
            .WithoutStrictOrdering());
    }
}