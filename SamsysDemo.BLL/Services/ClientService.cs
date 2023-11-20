using AutoMapper;
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
        private readonly IMapper mapper;

        public ClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            // TODO: criar automapper padrão para o projeto ao invés deste personalizado
            mapper = new MapperConfiguration(cfg =>
               cfg.CreateMap<Client, ClientDTO>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty))
               .ForMember(dest => dest.ConcurrencyToken, opt => opt.MapFrom(src => Convert.ToBase64String(src.ConcurrencyToken)))
               .ForMember(dest => dest.DataNascimento, opt => opt.MapFrom(src => src.DataNascimento.ToString("yyyy-MM-dd")))
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)))
               .CreateMapper();
        }
        public async Task<MessagingHelper<PaginatedList<ClientDTO>>> ListAsync(int skip, int take)
        {
            MessagingHelper<PaginatedList<ClientDTO>> response = new();

            var validacao = ValidarListarClientes(skip, take);
            if (validacao.Message.Count() > 0)
            {
                response.SetMessage($"Não foi possível listar os clientes. ERRO: {validacao.Message}");
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
                        DataNascimento = client.DataNascimento.ToString("yyyy-MM-dd")
                    };
                    lista.Add(clientdto);
                }
                var paginatedClients = new PaginatedList<ClientDTO>();
                paginatedClients.Items.AddRange(lista);
                paginatedClients.CurrentPage = skip;
                paginatedClients.PageSize = take;
                paginatedClients.TotalRecords = _unitOfWork.ClientRepository.TotalRegistro(); 


                response.Obj = paginatedClients;
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

                response.Obj = mapper.Map<ClientDTO>(client);
                //response.Obj = new ClientDTO
                //{
                //    Id = client.Id,
                //    IsActive = client.IsActive,
                //    ConcurrencyToken = Convert.ToBase64String(client.ConcurrencyToken),
                //    Name = client.Name,
                //    PhoneNumber = client.PhoneNumber,
                //    DataNascimento = client.DataNascimento.ToString("yyyy-MM-dd")
                //};
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
            MessagingHelper<ClientDTO> response = new();

            var validacao = ValidacaoCreate(clientDTO);
            if (validacao.Message.Length > 0)
            {
                response.SetMessage($"Não foi possível inserir o cliente. ERRO: {validacao.Message}");
                response.Success = false;
                return response;
            }
            try
            {
                //TODO - criar mapeamento de DTO para Client
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
                response.Obj = mapper.Map<ClientDTO>(client);
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
                client.Update(clientToUpdate.Name, clientToUpdate.PhoneNumber, clientToUpdate.DataNascimento);
                _unitOfWork.ClientRepository.Update(client, clientToUpdate.ConcurrencyToken);
                await _unitOfWork.SaveAsync();

                var clientMap = mapper.Map<ClientDTO>(client);
                response.Success = true;
                response.Obj = clientMap;
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
        private static MessagingHelper ValidacaoCreate(CreateClientDTO client)
        {
            MessagingHelper<ClientDTO> response = new();
            if (string.IsNullOrEmpty(client.Name))
                response.SetMessage("O campo 'Nome do Cliente' não pode estar vazio.");
            if (client.DataNascimento > DateTime.Now)
                response.SetMessage("O data de nascimento não pode ser maior que a data atual.");
            if (string.IsNullOrEmpty(client.PhoneNumber))
                response.SetMessage("O campo 'Número do Cliente' não pode estar vazio.");

            return response;
          
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
