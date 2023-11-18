using Microsoft.AspNetCore.Mvc;
using SamsysDemo.BLL.Services;
using SamsysDemo.Infrastructure.Helpers;
using SamsysDemo.Infrastructure.Models.Client;

namespace SamsysDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("Create")]
    public async Task<MessagingHelper> CreateAsync([FromBody] CreateClientDTO request)
    {
        return await _clientService.CreateClient(request);
    }

    /// <summary>
    /// Lista todos os clientes com paginação.
    /// </summary>
    /// <param name="pageNumber"></param> 
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("pageNumber/{pageNumber:int}/pageSize{pageSize:int}")]
    public async Task<MessagingHelper<PaginatedList<ClientDTO>>> ListAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _clientService.ListAsync(pageNumber, pageSize);
    }

    [HttpGet("{id}")]
    public async Task<MessagingHelper<ClientDTO>> Get(long id)
    {
        return await _clientService.Get(id);
    }

    [HttpPut("{id}")]
    public async Task<MessagingHelper> Update(int id, UpdateClientDTO clientToUpdateDTO)
    {
        return await _clientService.Update(id, clientToUpdateDTO);
    }

    [HttpPost("{id}/[action]")]
    public async Task<MessagingHelper> Enable(long id)
    {
        return await _clientService.EnableClient(id);
    }

    [HttpPost("{id}/[action]")]
    public async Task<MessagingHelper> Disable(long id)
    {
        return await _clientService.DisableClient(id);
    }
}
