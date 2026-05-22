using FluentAssertions;

namespace DataAggregator.Unit.Tests.Abstractions
{
    public class ArrangeContext<TTestClass, TBaseClass, TRequest, TResult>
        where TTestClass : BaseUnitTest<TTestClass, TBaseClass>
    {
        private readonly TTestClass _testClass;
        private readonly TRequest _request = default!;
        private readonly TResult? _expected;
        private TResult _result = default!;

        public ArrangeContext(TTestClass testClass, TRequest request, TResult? expected)
        {
            _testClass = testClass;
            _request = request;
            _expected = expected;
        }

        public ArrangeContext<TTestClass, TBaseClass, TRequest, TResult> Act(Func<TBaseClass, TRequest, TResult> actCallback)
        {
            try
            {
                _result = actCallback(_testClass._base, _request);
            }
            catch (Exception ex)
            {
                _testClass._exception = ex;
            }

            return this;
        }

        public void Assert(Action<TResult> assertion)
        {
            _testClass.Assert();

            if (_expected is not null)
            {
                _result
                    .Should()
                    .BeEquivalentTo(_expected);
            }

            assertion(_result);
        }

        public void AssertThrows<TException>(Action<TException> assertion)
            where TException : Exception
        {
            if (_expected is not null)
            {
                throw new InvalidOperationException($"Expected output is defined in {nameof(_testClass.Arrange)}. " +
                    $"Did you mean to call {nameof(Assert)} instead of {nameof(AssertThrows)}?");
            }

            _testClass._exception
                .Should()
                .NotBeNull();

            _testClass._exception
                .Should()
                .BeOfType<TException>();

            assertion((TException)_testClass._exception);
        }
     }
}
