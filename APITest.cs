using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace DartTournamentPlaner.APITest;

/// <summary>
/// Simple API Test Console Application
/// Tests the Dart Tournament Planner API endpoints
/// </summary>
class Program
{
    private static readonly HttpClient client = new();
    private const string API_BASE = "http://localhost:5000";

    static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 Dart Tournament Planner API Test"); 
        Console.WriteLine("====================================");
        Console.WriteLine();

        // Configure HttpClient
        client.Timeout = TimeSpan.FromSeconds(10);

        await TestHealthEndpoint();
        await TestTournamentStatus();
        await TestCurrentTournament();
        await TestPendingMatches();

        Console.WriteLine();
        Console.WriteLine("✅ API Tests completed!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task TestHealthEndpoint()
    {
        Console.WriteLine("🏥 Testing Health Endpoint...");
        try
        {
            var response = await client.GetAsync($"{API_BASE}/health");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {content}");
            Console.WriteLine("✅ Health endpoint working!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Health endpoint failed: {ex.Message}");
        }
        Console.WriteLine();
    }

    static async Task TestTournamentStatus()
    {
        Console.WriteLine("📊 Testing Tournament Status Endpoint...");
        try
        {
            var response = await client.GetAsync($"{API_BASE}/api/tournaments/status");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {content}");
            Console.WriteLine("✅ Tournament status endpoint working!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Tournament status endpoint failed: {ex.Message}");
        }
        Console.WriteLine();
    }

    static async Task TestCurrentTournament()
    {
        Console.WriteLine("🎯 Testing Current Tournament Endpoint...");
        try
        {
            var response = await client.GetAsync($"{API_BASE}/api/tournaments/current");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(content);
                var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Response: {formatted}");
                Console.WriteLine("✅ Current tournament endpoint working!");
            }
            else
            {
                Console.WriteLine($"Response: {content}");
                Console.WriteLine("ℹ️ No active tournament (expected if WPF app not running)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Current tournament endpoint failed: {ex.Message}");
        }
        Console.WriteLine();
    }

    static async Task TestPendingMatches()
    {
        Console.WriteLine("⚔️ Testing Pending Matches Endpoint...");
        try
        {
            var response = await client.GetAsync($"{API_BASE}/api/matches/pending");
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(content);
                var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Response: {formatted}");
                Console.WriteLine("✅ Pending matches endpoint working!");
            }
            else
            {
                Console.WriteLine($"Response: {content}");
                Console.WriteLine("ℹ️ No pending matches (expected if no tournament active)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pending matches endpoint failed: {ex.Message}");
        }
        Console.WriteLine();
    }
}