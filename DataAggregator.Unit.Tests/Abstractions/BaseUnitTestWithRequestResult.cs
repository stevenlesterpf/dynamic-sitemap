using FluentAssertions;

namespace DataAggregator.Unit.Tests.Abstractions
{
    public abstract class BaseUnitTestWithRequestResult<TTestClass, TBaseClass, TRequest, TResult>
            : BaseUnitTest<TTestClass, TBaseClass>
        where TTestClass : BaseUnitTestWithRequestResult<TTestClass, TBaseClass, TRequest, TResult>
    {
        private TRequest _request = default!;
        private TResult _expected = default!;
        private TResult _result = default!;

        public TTestClass Arrange(
            Action<TRequest> arrangeRequest,
            Action<TResult>? arrangeExpected = null)
        {
            _request = Activator.CreateInstance<TRequest>();
            arrangeRequest(_request);

            if (arrangeExpected is not null)
            {
                arrangeExpected(_expected);
            }

            return (TTestClass)this;
        }

        public void Assert(Action<TResult> assertion)
        {
            base.Assert();

            if (_expected is not null)
            {
                _result
                    .Should()
                    .BeEquivalentTo(_expected);
            }

            assertion(_result);
        }
    }
}
