namespace AIService.RAG
{
    public class KnowledgeBase
    {
        private readonly InMemoryVectorStore _store = new();

        public KnowledgeBase()
        {
            Seed();
        }

        public List<VectorDocument> Retrieve(string query, int topK = 3, string? category = null) =>
            _store.Search(query, topK, category);

        private void Seed()
        {
            // ── Booking FAQs ──────────────────────────────────────────────────
            _store.AddDocument("faq-1", "To create a booking, search for available flights, select a flight, choose your seat, and proceed to payment. You need to be logged in.", "faq");
            _store.AddDocument("faq-2", "To cancel a booking, go to My Bookings and click Cancel. Cancellations are allowed before the flight departure.", "faq");
            _store.AddDocument("faq-3", "You can download your ticket as a PDF from My Bookings by clicking Download Ticket.", "faq");
            _store.AddDocument("faq-4", "Booking confirmation is sent to your registered email with a PDF ticket attached.", "faq");
            _store.AddDocument("faq-5", "A booking reference number is generated when you complete your booking. It starts with BK- followed by the date and a unique code.", "faq");

            // ── Seat FAQs ─────────────────────────────────────────────────────
            _store.AddDocument("seat-1", "Economy class seats are standard seats available on all flights. Business class seats offer more legroom and premium service.", "faq");
            _store.AddDocument("seat-2", "Emergency exit seats have extra legroom but passengers must be able to assist in an emergency.", "faq");
            _store.AddDocument("seat-3", "Seat selection is available after choosing a flight. You can view the seat map showing available, locked, and confirmed seats.", "faq");
            _store.AddDocument("seat-4", "A seat lock expires after 15 minutes if payment is not completed. The seat will be released automatically.", "faq");
            _store.AddDocument("seat-5", "Window seats are in columns A and F. Aisle seats are in columns C and D. Middle seats are B and E.", "faq");

            // ── Flight FAQs ───────────────────────────────────────────────────
            _store.AddDocument("flight-1", "To search flights, you can search by origin and destination cities. Example: 'Show me flights from Delhi to Mumbai' or 'Flights from BOM to DEL on 2025-01-15'.", "faq");
            _store.AddDocument("flight-2", "Flight status can be Scheduled, Delayed, Cancelled, or Completed.", "faq");
            _store.AddDocument("flight-3", "FlightOps operates flights between major Indian cities: Delhi, Mumbai, Bangalore, Chennai, and Hyderabad.", "faq");
            _store.AddDocument("flight-4", "DEL is Delhi Indira Gandhi International Airport. BOM is Mumbai Chhatrapati Shivaji Airport. BLR is Bangalore Kempegowda Airport. MAA is Chennai International Airport. HYD is Hyderabad Rajiv Gandhi Airport.", "faq");
            _store.AddDocument("flight-5", "Each flight is operated by a specific aircraft. The aircraft determines the total number of seats and seat layout.", "faq");
            _store.AddDocument("flight-6", "You can search flights by city name or airport code. For example: 'Delhi' or 'DEL', 'Mumbai' or 'BOM'.", "faq");
            _store.AddDocument("flight-7", "Flight search shows pricing for Economy, Premium Economy, Business, and First Class. Child passengers get 25% discount, infants get 90% discount.", "faq");
            _store.AddDocument("flight-8", "You can filter flights by price range, time of day (morning, afternoon, evening), and sort by price, duration, or departure time.", "faq");
            _store.AddDocument("flight-9", "Round trip bookings are supported. Search for outbound and return flights together by specifying both departure and return dates.", "faq");

            // ── Pricing FAQs ──────────────────────────────────────────────────
            _store.AddDocument("price-1", "Flight prices vary by class: Economy (base price), Premium Economy (higher), Business (premium), and First Class (luxury).", "faq");
            _store.AddDocument("price-2", "Children (2-11 years) receive 25% discount on all ticket prices. Infants (0-23 months) receive 90% discount.", "faq");
            _store.AddDocument("price-3", "You can calculate exact price before booking using the calculate-price endpoint with passenger details and seat classes.", "faq");
            _store.AddDocument("price-4", "Prices include base fare plus 18% taxes. The total amount is shown in the booking confirmation.", "faq");

            // ── Payment FAQs ──────────────────────────────────────────────────
            _store.AddDocument("pay-1", "Payments are processed through Razorpay. You need to create a payment order and verify the payment after completion.", "faq");
            _store.AddDocument("pay-2", "After successful payment, your booking status changes to Confirmed and your seat is confirmed.", "faq");
            _store.AddDocument("pay-3", "If payment fails, your seat lock is released and the booking is marked as Failed. You can try again.", "faq");
            _store.AddDocument("pay-4", "Refunds are processed by admin only. Contact support with your booking reference for refund requests.", "faq");

            // ── Account FAQs ──────────────────────────────────────────────────
            _store.AddDocument("acc-1", "To register, use POST /auth/register with your name, email, password, and role.", "faq");
            _store.AddDocument("acc-2", "To login, use POST /auth/login with your email and password. You will receive a JWT token.", "faq");
            _store.AddDocument("acc-3", "Admin users can manage flights, aircraft, airports, routes, and process refunds.", "faq");
            _store.AddDocument("acc-4", "Regular users can search flights, create bookings, select seats, and download tickets.", "faq");
            _store.AddDocument("acc-5", "Your travel profile stores your full name, age, gender, and passport number for quick booking.", "faq");
        }
    }
}
