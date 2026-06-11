using System.Text.Json;
using FufuLauncher.Contracts.Services;
using FufuLauncher.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FufuLauncher.Services;

public class AccountManager
{

    private readonly string _dataDir;
    private readonly string _cookiesDir;
    private readonly string _accountsFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private AccountList _accountList;
    private string? _activeAccountId;
    public string? ActiveAccountId => _activeAccountId;
    public AccountManager()
    {
        _dataDir = Helpers.AppPaths.DataDir;
        _cookiesDir = Path.Combine(_dataDir, "cookies");
        _accountsFilePath = Path.Combine(_dataDir, "accounts.json");

        Directory.CreateDirectory(_cookiesDir);
        _accountList = new AccountList();
    }

    public async Task InitializeAsync()
    {
        await LoadAccountListAsync();
    }



    public AccountEntry GetActiveAccountEntry() =>
        _accountList.Accounts.FirstOrDefault(a => a.Id == _activeAccountId);

    public List<AccountEntry> GetAllAccounts() => _accountList.Accounts;

  
    private async Task LoadAccountListAsync()
    {
        if (File.Exists(_accountsFilePath))
        {
            var json = await File.ReadAllTextAsync(_accountsFilePath);
            _accountList = JsonSerializer.Deserialize<AccountList>(json) ?? new AccountList();
        }
        else
        {
            _accountList = new AccountList();
        }

        var settings = App.GetService<ILocalSettingsService>();
        try
        {
            var savedObj = await settings.ReadSettingAsync("ActiveAccountId");
            var savedId = savedObj as string;
            _activeAccountId = savedId ?? _accountList.Accounts.FirstOrDefault()?.Id;
        }
        catch
        {
            
            _activeAccountId = _accountList.Accounts.FirstOrDefault()?.Id;
        }
    }
    public async Task SetActiveAccountIdAsync(string? accountId)
    {
        _activeAccountId = accountId;
        var settings = App.GetService<ILocalSettingsService>();
        if (settings != null)
            await settings.SaveSettingAsync("ActiveAccountId", accountId ?? string.Empty);
    }
    public async Task LogoutAsync()
    {
        await SetActiveAccountIdAsync(null);
    }
    private async Task SaveAccountListAsync()
    {
        var json = JsonSerializer.Serialize(_accountList, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_accountsFilePath, json);
    }


    public async Task<AccountEntry> AddAccountAsync(
        Dictionary<string, string> cookies, string serverType, string nickname = "")
    {
        await _lock.WaitAsync();
        try
        {
            string stuid = ExtractStuid(cookies, serverType);
            string id = $"{serverType}_{stuid}";

            if (_accountList.Accounts.Any(a => a.Id == id))
                throw new InvalidOperationException("该账户已存在");

            string cookieFileName = $"{id}.json";
            string cookiePath = Path.Combine(_cookiesDir, cookieFileName);
            var cookieJson = JsonSerializer.Serialize(cookies);
            await File.WriteAllTextAsync(cookiePath, cookieJson);

            var entry = new AccountEntry
            {
                Id = id,
                Stuid = stuid,
                Nickname = nickname,
                ServerType = serverType,
                CookieFilePath = cookieFileName,
                LastLoginTime = DateTime.Now
            };

            _accountList.Accounts.Add(entry);
            await SaveAccountListAsync();
            return entry;
        }
        finally
        {
            _lock.Release();
        }
    }


    public async Task<Dictionary<string, string>> LoadCookiesAsync(string accountId)
    {
        var entry = _accountList.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (entry == null) return null;

        string path = Path.Combine(_cookiesDir, entry.CookieFilePath);
        if (!File.Exists(path)) return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }


    public async Task DeleteAccountAsync(string accountId)
    {
        await _lock.WaitAsync();
        try
        {
            var entry = _accountList.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (entry == null) return;

            string path = Path.Combine(_cookiesDir, entry.CookieFilePath);
            if (File.Exists(path)) File.Delete(path);

            _accountList.Accounts.Remove(entry);
            await SaveAccountListAsync();

            if (_activeAccountId == accountId)
            {
                var next = _accountList.Accounts.FirstOrDefault();
                await SetActiveAccountIdAsync(next?.Id);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

   
    public async Task<bool> SwitchAccountAsync(string accountId)
    {
        if (_accountList.Accounts.All(a => a.Id != accountId)) return false;
        await SetActiveAccountIdAsync(accountId);

        var entry = GetActiveAccountEntry();
        if (entry != null)
        {
            entry.LastLoginTime = DateTime.Now;
            await SaveAccountListAsync();
        }
        return true;
    }

   
    public async Task UpdateAccountMetaAsync(string accountId, string nickname, string avatarUrl)
    {
        await _lock.WaitAsync();
        try
        {
            var entry = _accountList.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (entry != null)
            {
                entry.Nickname = nickname;
                entry.AvatarUrl = avatarUrl;
                await SaveAccountListAsync();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    
    private string ExtractStuid(Dictionary<string, string> cookies, string serverType)
    {
        if (serverType == "cn")
        {
            if (cookies.TryGetValue("ltuid", out var ltuid)) return ltuid;
            if (cookies.TryGetValue("stuid", out var stuid)) return stuid;
        }
        else
        {
            if (cookies.TryGetValue("ltuid_v2", out var ltuidV2)) return ltuidV2;
        }
        throw new ArgumentException("无法提取账户 ID");
    }
    public async Task UpdateCookiesAsync(string accountId, Dictionary<string, string> newCookies)
    {
        await _lock.WaitAsync();
        try
        {
            var entry = _accountList.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (entry == null) return;

            string cookiePath = Path.Combine(_cookiesDir, entry.CookieFilePath);
            var json = JsonSerializer.Serialize(newCookies);
            await File.WriteAllTextAsync(cookiePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

}