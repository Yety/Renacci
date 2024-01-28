using Core;

namespace Renacci.UseCases;

public class GetStatusUseCase(IStatusRepository _statusRepository)
{
    
    public async Task<InverterStatus> GetStatus()
    {
        var status = await _statusRepository.GetStatus();
        return status;
    }
}