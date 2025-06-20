using Grpc.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Services.Models;

public interface IUserRepository
{
    Task<User> CreateAsync(string name, string email, string password);
    Task<User> GetByEmailAsync(string email);
}

public class FileUserRepository : IUserRepository
{
    private const string FilePath = "/data/users.json";
    private List<User> _store;
    private long _nextId;

    public FileUserRepository()
    {
        if (File.Exists(FilePath))
            _store = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(FilePath)) ?? new();
        else
            _store = new();

        _nextId = _store.Count > 0 ? (_store.Max(u => u.Id) + 1) : 1;
    }

    public async Task<User> CreateAsync(string name, string email, string password)
    {
        if(_store.Any(a => a.Email == email)) return null;

        var user = new User { Id = _nextId++, Name = name, Email = email, Password = password };

        _store.Add(user);
        Persist();
        return user;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var user = _store.FirstOrDefault(f => f.Email == email);
        return user;
    }

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(_store, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}