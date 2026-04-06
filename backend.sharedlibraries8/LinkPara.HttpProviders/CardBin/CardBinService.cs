using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Response;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.CardBin
{
	public class CardBinService : HttpClientBase, ICardBinService
	{
		public CardBinService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
		{
		}

		public async Task<IKSResponse<CardBinResponse>> GetCardBinRangeAsync()
		{
			var response = await GetAsync($"v1/CardBin/cardBinRanges");

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException();
			}

			var cardBinResponse = await response.Content.ReadFromJsonAsync<IKSResponse<CardBinResponse>>();

			return cardBinResponse ?? throw new InvalidCastException();
		}
	}
}
