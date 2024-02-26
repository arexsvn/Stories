using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UserApi : IUserApi
{
    readonly WebRequestService _webRequestService;
    public UserApi(WebRequestService webRequestService)
    {
        _webRequestService = webRequestService;
    }

    public async Task<GetUserResponse> GetUser()
    {
        CancellationTokenSource cancellation = new CancellationTokenSource();
        //cancellation.Cancel();
        //cancellation.CancelAfter(1300);
        //CancelAfter(cancellation);
        GetUserResponse response = await _webRequestService.Get<GetUserResponse>("https://api.restful-api.dev/objects/4", cancellation);

        if (response != default)
        {
            Debug.Log($"webResponse : id : {response.id} color : {response.data.color}");
        }

        return response;
    }

    private async Task CancelAfter(CancellationTokenSource token)
    {
        await Awaitable.WaitForSecondsAsync(.1f);
        token.Cancel();
        Debug.Log("CANCELLING!");
    }
}

public class MockUserApi : IUserApi
{
    public async Task<GetUserResponse> GetUser()
    {
        GetUserResponse response = new GetUserResponse();
        response.id = 123;
        response.name = "Tester";

        await Awaitable.WaitForSecondsAsync(1.5f);

        return response;
    }
}

public interface IUserApi
{
    Task<GetUserResponse> GetUser();
}

// {"id":"4","name":"Apple iPhone 11, 64GB","data":{"price":389.99,"color":"Purple"}}
public class GetUserResponse
{
    public int id;
    public string name;
    public CarData data;
}

public class CarData
{
    public string price;
    public string color;
}