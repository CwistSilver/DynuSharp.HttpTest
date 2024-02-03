using DynuSharp.Data.Dns;
using DynuSharp.Data.Dns.Record;
using DynuSharp.HttpTest.Data;
using DynuSharp.HttpTest.Utilities;
using System.Text.Json;

namespace DynuSharp.HttpTest.Client.Dns;
internal class DnsRecordHttpTest : IHttpTest
{
    private const string TestName = "DNS Record";
    private readonly IDynuClient _client;
    private readonly HttpTestGroup _httpTestGroup;
    private readonly DnsDomain _dnsDomain;

    internal DnsRecordHttpTest(IDynuClient client, TestContainer container, DnsDomain dnsDomain)
    {
        _client = client;

        _dnsDomain = dnsDomain;
        _httpTestGroup = new HttpTestGroup(TestName);
        container.Results.Add(_httpTestGroup);
    }

    public async Task Run()
    {
        var record = await CreateRecord();
        if (record is not null)
        {
            await GetAllRecordsOfTypeDnsRecordA(record);
            await GetAllRecordsOfTypeDnsRecordAGeneric(record);
            await UpdateRecord(record);
        }

        var records = await GetAllRecords();

        DnsRecordBase? selectedRecord = record;
        if (record is null && records is not null && records.Count != 0)
            selectedRecord = records[0];

        if (selectedRecord is null) return;

        await GetRecordUsingGeneric(selectedRecord.Id);
        await GetRecordWithoutGeneric(selectedRecord.Id);

        if (record is not null)
            await DeleteRecordDetails(record);
    }

    private async Task<IReadOnlyList<DnsRecordBase>?> GetAllRecords()
    {
        var jsonDatas = new List<JsonElement>();
        IReadOnlyList<DnsRecordBase>? records = null;

        await _httpTestGroup.AddTest(
            testName: $"Get DNS records list for domain: {_dnsDomain.Id} ({_dnsDomain.Name})",
            testFunction: async () =>
            {
                records = await _client.DNS.Records.GetListAsync(_dnsDomain.Id);
                foreach (var record in records)
                    jsonDatas.Add(JsonSerializer.SerializeToElement(record, record.GetType(), GlobalJsonOptions.Options));
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(jsonDatas, GlobalJsonOptions.Options)
         );

        return records;
    }

    private async Task GetAllRecordsOfTypeDnsRecordA(DnsRecordBase record)
    {
        var jsonDatas = new List<JsonElement>();
        IReadOnlyList<DnsRecordBase>? recordsOfTypeA = null;

        await _httpTestGroup.AddTest(
            testName: $"Get DNS records list of type {nameof(DnsRecordA)} for domain host: {record.DomainName}",
            testFunction: async () =>
            {
                recordsOfTypeA = await _client.DNS.Records.GetListOfTypeAsync(record.DomainName, RecordType.A);
                foreach (var record in recordsOfTypeA)
                    jsonDatas.Add(JsonSerializer.SerializeToElement(record, record.GetType(), GlobalJsonOptions.Options));
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(jsonDatas, GlobalJsonOptions.Options)
         );
    }

    private async Task GetAllRecordsOfTypeDnsRecordAGeneric(DnsRecordBase record)
    {
        IReadOnlyList<DnsRecordA>? recordsOfTypeA = null;

        await _httpTestGroup.AddTest(
            testName: $"Get generic DNS records list of type {nameof(DnsRecordA)} for domain host: {record.DomainName}",
            testFunction: async () =>
            {
                recordsOfTypeA = await _client.DNS.Records.GetListOfTypeAsync<DnsRecordA>(record.DomainName);
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(recordsOfTypeA, GlobalJsonOptions.Options)
         );
    }

    private async Task<DnsRecordBase?> CreateRecord()
    {
        DnsRecordBase? addedDnsRecord = null;

        await _httpTestGroup.AddTest(
            testName: $"Add record Async for domain: {_dnsDomain.Id} ({_dnsDomain.Name})",
            testFunction: async () =>
            {
                addedDnsRecord = new DnsRecordA() { IPv4Address = "89.243.18.161" };
                addedDnsRecord = (DnsRecordA)await _client.DNS.Records.AddAsync(_dnsDomain.Id, addedDnsRecord);
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(addedDnsRecord, GlobalJsonOptions.Options)
        );
        return addedDnsRecord;
    }


    private async Task UpdateRecord(DnsRecordBase dnsRecordBase)
    {
        DnsRecordA? recivedRecord = null;

        await _httpTestGroup.AddTest(
            testName: $"Update record for domain: {_dnsDomain.Id} ({_dnsDomain.Name})",
            testFunction: async () =>
            {
                ((DnsRecordA)dnsRecordBase).IPv4Address = "65.166.23.200";
                recivedRecord = (DnsRecordA)await _client.DNS.Records.UpdateAsync(_dnsDomain.Id, dnsRecordBase.Id, dnsRecordBase);
                if (recivedRecord.IPv4Address != ((DnsRecordA)dnsRecordBase).IPv4Address)
                {
                    throw new Exception("Not updated!");
                }
            },
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(recivedRecord, recivedRecord!.GetType(), GlobalJsonOptions.Options)
        );
    }

    private async Task DeleteRecordDetails(DnsRecordBase dnsRecordBase)
    {
        await _httpTestGroup.AddTest(
            testName: $"Delete record: {dnsRecordBase.Id}",
            testFunction: async () => await _client.DNS.Records.DeleteAsync(_dnsDomain.Id, dnsRecordBase.Id)
          );
    }

    private async Task GetRecordUsingGeneric(int recordId)
    {
        DnsRecordBase? recordDetailsGeneric = null;

        await _httpTestGroup.AddTest(
            testName: $"Get record with the ID '{recordId}' using generic method",
            testFunction: async () => recordDetailsGeneric = await _client.DNS.Records.GetAsync<DnsRecordBase>(_dnsDomain.Id, recordId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(recordDetailsGeneric, recordDetailsGeneric!.GetType(), GlobalJsonOptions.Options)
          );
    }

    async Task GetRecordWithoutGeneric(int recordId)
    {
        DnsRecordBase? recordDetails = null;

        await _httpTestGroup.AddTest(
            testName: $"Get record with the ID '{recordId}' without generic method",
            testFunction: async () => recordDetails = await _client.DNS.Records.GetAsync<DnsRecordBase>(_dnsDomain.Id, recordId),
            onSuccess: httpTestData => httpTestData.Data = JsonSerializer.SerializeToElement(recordDetails, recordDetails!.GetType(), GlobalJsonOptions.Options)
        );
    }
}
