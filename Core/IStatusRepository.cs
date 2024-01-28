using Renacci;

namespace Core;

public interface IStatusRepository
{
    Task<InverterStatus> GetStatus();
}