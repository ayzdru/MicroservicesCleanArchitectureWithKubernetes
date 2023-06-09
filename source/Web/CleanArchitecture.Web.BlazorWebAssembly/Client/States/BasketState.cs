﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Basket.API.Grpc.V1;

namespace CleanArchitecture.Web.BlazorWebAssembly.Client.States
{

    public class BasketState
    {

        public Dictionary<string, int> OldBasketItemQuantities
        {
            get;
            set;
        }
        private IList<BasketItemResponse>? _basketItems = null;

        public IList<BasketItemResponse> BasketItems {
            get
            {

                return _basketItems;
            }
            set
            {
                _basketItems = value;
                OldBasketItemQuantities = new Dictionary<string, int>();
                foreach (var basketItem in value)
                {
                    OldBasketItemQuantities.Add(basketItem.ProductId, basketItem.Quantity);
                }
                BasketItemsCount = BasketItems.Count;
            }
        } 
        private int _basketItemsCount;
        public int BasketItemsCount
        {
            get
            {
                return _basketItemsCount;
            }
            set
            {
                _basketItemsCount = value;
                NotifyStateChanged();
            }
        }
        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
