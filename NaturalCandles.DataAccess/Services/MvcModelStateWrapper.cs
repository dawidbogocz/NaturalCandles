using Microsoft.AspNetCore.Mvc.ModelBinding;
using NaturalCandles.DataAccess.Services.IServices;

namespace NaturalCandles.DataAccess.Services
{
    public class MvcModelStateWrapper : ModelStateDictionaryWrapper
    {
        private readonly ModelStateDictionary _modelState;

        public MvcModelStateWrapper(ModelStateDictionary modelState)
        {
            _modelState = modelState;
        }

        public void AddModelError(string key, string errorMessage)
        {
            _modelState.AddModelError(key, errorMessage);
        }
    }
}