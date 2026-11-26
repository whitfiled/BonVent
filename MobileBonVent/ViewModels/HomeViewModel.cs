using MobileBonVent.Models;
using System.Collections.ObjectModel;

namespace MobileBonVent.ViewModels;

internal class HomeViewModel : BaseViewModel
{
    private string _hotelName = string.Empty;
    private string _guestName = string.Empty;
    private int _notificationCount;
    private Reservation? _currentReservation;

    public string HotelName
    {
        get => _hotelName; set => SetProperty(ref _hotelName, value);
    }

    public string GuestName
    {
        get => _guestName; set => SetProperty(ref _guestName, value);
    }

    public int NotificationCount
    {
        get => _notificationCount; set => SetProperty(ref _notificationCount, value);
    }

    public Reservation? CurrentReservation
    {
        get => _currentReservation; set => SetProperty(ref _currentReservation, value);
    }

    public ObservableCollection<Room> FeaturedRooms { get; } = [];

    public ObservableCollection<Offer> FeaturedOffers { get; } = [];

    public HomeViewModel()
    {
        Title = "Home";
        HotelName = "HotelBonVent";
        GuestName = "Guest";
        NotificationCount = 0;
    }
}