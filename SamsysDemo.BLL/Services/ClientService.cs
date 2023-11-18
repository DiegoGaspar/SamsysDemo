using Microsoft.EntityFrameworkCore;
using SamsysDemo.Infrastructure.Entities;
using SamsysDemo.Infrastructure.Helpers;
using SamsysDemo.Infrastructure.Interfaces.Repositories;
using SamsysDemo.Infrastructure.Models.Client;

namespace SamsysDemo.BLL.Services
{


    public class ClientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<MessagingHelper<PaginatedList<ClientDTO>>> ListAsync(int skip, int take)
        {
            MessagingHelper<PaginatedList<ClientDTO>> response = new();

            var validacao = ValidarListarClientes(skip, take);
            if (validacao.Message.Count() > 0)
            {
                response.SetMessage($"Não foi possível listar os clientes. {validacao.Message}");
                response.Success = false;
                return response;
            }

            try
            {
                List<ClientDTO> lista = new();
                var clients = await _unitOfWork.ClientRepository.ListAll(skip,take);
                if (clients.Count == 0 )
                {
                    response.SetMessage($"Não há clientes ativos cadastrados.");
                    response.Success = true;
                    return response;
                }
                foreach (var client in clients)
                {
                    var clientdto = new ClientDTO
                    {
                        Id = client.Id,
                        IsActive = client.IsActive,
                        ConcurrencyToken = Convert.ToBase64String(client.ConcurrencyToken),
                        Name = client.Name,
                        PhoneNumber = client.PhoneNumber, 
                        DataNascimento = client.DataNascimento
                    };
                    lista.Add(clientdto);
                }
                var paginatedDinners = new PaginatedList<ClientDTO>();
                paginatedDinners.Items.AddRange(lista);
                paginatedDinners.CurrentPage = skip;
                paginatedDinners.PageSize = take;
                paginatedDinners.TotalRecords = _unitOfWork.ClientRepository.TotalRegistro(); 


                response.Obj = paginatedDinners;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro tentar listar os clientes. ERROR: {ex}");
                return response;
            }
        }
        public async Task<MessagingHelper<ClientDTO>> Get(long id)
        {
            MessagingHelper<ClientDTO> response = new();
            try
            {
                Client? client = await _unitOfWork.ClientRepository.GetById(id);
                if (client is null)
                {
                    response.SetMessage($"O cliente não existe. | Id: {id}");
                    response.Success = false;
                    return response;
                }
                response.Obj = new ClientDTO
                {
                    Id = client.Id,
                    IsActive = client.IsActive,
                    ConcurrencyToken = Convert.ToBase64String(client.ConcurrencyToken),
                    Name = client.Name,
                    PhoneNumber = client.PhoneNumber,
                    DataNascimento = client.DataNascimento
                };
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro inesperado ao obter o cliente.");
                return response;
            }
        }
        public async Task<MessagingHelper> CreateClient(CreateClientDTO clientDTO)
        {
            MessagingHelper<Client> response = new();

            var validacao = ValidacaoCreate(clientDTO);
            if (!validacao)
            {
                response.SetMessage($"Não foi possível inserir o cliente.");
                response.Success = false;
                return response;
            }
            try
            {
                //TODO - no meu entendimento o ideal seria trabalhar com mapeamento nesse caso.
                var client = new Client
                {
                    Name = clientDTO.Name,
                    DataNascimento = clientDTO.DataNascimento,
                    PhoneNumber = clientDTO.PhoneNumber,
                    IsActive = true,
                    IsRemoved = false
                };

                await _unitOfWork.ClientRepository.Insert(client);
                await _unitOfWork.SaveAsync();
                response.Success = true;
                response.Obj = client;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro inesperado ao tentar inserir o cliente. Tente novamente. ERROR:{ex}");
                return response;
            }
        }
        public async Task<MessagingHelper> Update(long id, UpdateClientDTO clientToUpdate)
        {
            MessagingHelper<Client> response = new();
            try
            {
                Client? client = await _unitOfWork.ClientRepository.GetById(id);
                if (client is null)
                {
                    response.SetMessage($"O cliente não existe. | Id: {id}");
                    response.Success = false;
                    return response;
                }
                client.Update(clientToUpdate.Name, clientToUpdate.PhoneNumber, clientToUpdate.DataNascimento);
                _unitOfWork.ClientRepository.Update(client, clientToUpdate.ConcurrencyToken);
                await _unitOfWork.SaveAsync();
                response.Success = true;
                response.Obj = client;
                return response;
            }
            catch (DbUpdateConcurrencyException exce)
            {
                response.Success = false;
                response.SetMessage($"Os dados do cliente foram atualizados posteriormente por outro utilizador!.");
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro inesperado ao atualizar o cliente. Tente novamente.");
                return response;
            }
        }
        public async Task<MessagingHelper> DisableClient(long id)
        {
            MessagingHelper<Client> response = new();
            try
            {
                Client? client = await _unitOfWork.ClientRepository.GetById(id);
                if (client is null)
                {
                    response.SetMessage($"O cliente não existe. | Id: {id}");
                    response.Success = false;
                    return response;
                }
                client.SetStatus(false);
                _unitOfWork.ClientRepository.Update(client, Convert.ToBase64String(client.ConcurrencyToken));
                await _unitOfWork.SaveAsync();
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro inativar o cliente.");
                return response;
            }
        }
        public async Task<MessagingHelper> EnableClient(long id)
        {
            MessagingHelper<Client> response = new();
            try
            {
                Client? client = await _unitOfWork.ClientRepository.GetById(id);
                if (client is null)
                {
                    response.SetMessage($"O cliente não existe. | Id: {id}");
                    response.Success = false;
                    return response;
                }
                client.SetStatus(true);
                _unitOfWork.ClientRepository.Update(client, Convert.ToBase64String(client.ConcurrencyToken));
                await _unitOfWork.SaveAsync();
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.SetMessage($"Ocorreu um erro ativar o cliente.");
                return response;
            }
        }
       
        #region Métodos auxiliares

        //TODO - no minha visão o ideal seria criar um conceito de validações retornando Notificações
        private static bool ValidacaoCreate(CreateClientDTO client)
        {
            if (string.IsNullOrEmpty(client.Name) ||
                client.DataNascimento > DateTime.Now ||
                string.IsNullOrEmpty(client.PhoneNumber))
            {
                return false;
            }
            return true;
        }
        private static MessagingHelper ValidarListarClientes(int skip, int take) 
        {
            MessagingHelper<PaginatedList<ClientDTO>> response = new();
            if (skip < 1)
                response.SetMessage($"O valor do número da página não pode ser menor que 1. Número informado: {skip}");
            
            if (take < 1)
                response.SetMessage($"O valor do número de dados apresentador não pode ser menor que 1. Número informado: {take}");

            return response;
        }

        #endregion
    }
}
