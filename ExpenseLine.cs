using System;

namespace ExpenseClaimConverter
{
    /// <summary>Represents one input expense line and its conversion result.</summary>
    public sealed class ExpenseLine
    {
        /// <summary>Gets or sets the source line identifier.</summary>
        public string LineId { get; set; }
        /// <summary>Gets or sets the unparsed source expense date.</summary>
        public string ExpenseDateText { get; set; }
        /// <summary>Gets or sets the source description.</summary>
        public string Description { get; set; }
        /// <summary>Gets or sets the unparsed source amount.</summary>
        public string AmountText { get; set; }
        /// <summary>Gets or sets the source currency code.</summary>
        public string Currency { get; set; }
        /// <summary>Gets or sets the exchange rate used for conversion.</summary>
        public decimal Rate { get; set; }
        /// <summary>Gets or sets the date returned by the rate service.</summary>
        public DateTime? RateDate { get; set; }
        /// <summary>Gets or sets the rounded converted amount.</summary>
        public decimal? ConvertedAmount { get; set; }
        /// <summary>Gets or sets the processing status.</summary>
        public string Status { get; set; }
        /// <summary>Gets or sets the business or service message.</summary>
        public string Message { get; set; }
    }
}
