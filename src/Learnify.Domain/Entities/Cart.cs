using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Domain.Entities;

public class Cart
{
    public Cart(string userId)
    {
        UserId = userId;
    }
    public string UserId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();

  
}