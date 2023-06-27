using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTM_MART.Models.ViewModels
{
	public  class ShoppingCartVM
	{
		public IEnumerable<ShoppingCart> ListCart { get; set; }
		public IEnumerable<ShoppingCart> SummaryCart { get; set; }
		//public double CartTotal { get; set; }
		public double VatTotal { get; set; }
		public double SubTotal { get; set; }
		public double RewardTotal { get; set; }

		public OrderHeader OrderHeader { get; set; }
	}
}
