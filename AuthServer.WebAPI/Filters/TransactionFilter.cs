using AuthServer.Application.Interfaces.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthServer.WebAPI.Filters
{
    public class TransactionFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(default);

            var resultContext = await next();

            if (resultContext.Exception != null)
                await transaction.RollbackAsync();
            else
            {
                await _unitOfWork.SaveChangesAsync(default);
                await transaction.CommitAsync();
            }
        }
    }

}
