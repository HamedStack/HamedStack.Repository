using Microsoft.EntityFrameworkCore;

namespace HamedStack.TheRepository.EntityFrameworkCore;

public static class ExceptionExtensions
{
    public static bool IsDbUpdateConcurrencyException(this Exception ex)
    {
        return ex is DbUpdateConcurrencyException;
    }
}