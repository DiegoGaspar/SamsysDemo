using Microsoft.AspNetCore.Mvc;
using SamsysDemo.BLL.Services;
using SamsysDemo.Infrastructure.Helpers;
using SamsysDemo.Infrastructure.Models.Client;

namespace SamsysDemo.Controllers
{
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
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public async Task<MessagingHelper> CreateAsync([FromBody] CreateClientDTO request)
        {
            return await _clientService.CreateClient(request);
        }


        /// <summary>
        /// Lista todos os cliente.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<MessagingHelper<List<ClientDTO>>> ListAsync()
        {
            //TODO -Criar paginação
            return await _clientService.ListAsync();
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
}
