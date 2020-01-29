using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs.common
{
    public interface INetJob : ObservableJob<INetworkListener>
    {
        Task ExecuteAsync();
    }
}
