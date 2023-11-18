﻿using SamsysDemo.Infrastructure.Entities;

namespace SamsysDemo.Infrastructure.Interfaces.Repositories
{
    public interface IClientRepository
    {
        Task<IList<Client>> ListAll();
        Task<Client?> GetById(object id, string[]? includedProperties = null);
        Task Insert(Client entityToInsert);
        void Update(Client entityToUpdate, string concurrencyToken);
        Task Delete(object id, string userDelete, string concurrencyToken);       
    }
}
