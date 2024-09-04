namespace Extraordinary.App.Respone
{
    public struct ResponeReturn<TResult>
    {
        public bool Succeed { set; get; }
        public TResult ResultValue { set; get; }
        public string ErrorValue { set; get; }

        public static ResponeReturn<TResult> NewOk(TResult r)
        {
            return new ResponeReturn<TResult> { Succeed = true, ResultValue = r };
        }
        public static ResponeReturn<TResult> NewError(string msg)
        {
            return new ResponeReturn<TResult> { Succeed = true, ErrorValue = msg };
        }
    }

    public static class ResponeReturnExtensions
    {
        public static ResponeReturn<TResult> ToOkResult<TResult>(this TResult r)
        {
            return ResponeReturn<TResult>.NewOk(r);
        }

        public static ResponeReturn<TResult> ToErrorResult<TResult>(this string r)
        {
            return ResponeReturn<TResult>.NewError(r);
        }

        public static Task<ResponeReturn<TResult>> ToTask<TResult>(this ResponeReturn<TResult> res)
        {
            return Task.FromResult(res);
        }
    }
}
