import { useState, useEffect, useRef } from 'react';
import { api } from './api';
import { CommentHubConnection } from './CommentHub';

// Import our professional components
import { Navbar } from './components/Navbar';
import { Hero } from './components/Hero';
import { CategoryFilters } from './components/CategoryFilters';
import { EventCard } from './components/EventCard';
import { AuthModal } from './components/AuthModal';
import { Pagination } from './components/Pagination';

// Type definitions matching backend schema
interface Category {
  id: string;
  name: string;
  description: string;
}

interface EventItem {
  id: string;
  title: string;
  description: string;
  location: string;
  latitude?: number;
  longitude?: number;
  date: string;
  categoryId: string;
  categoryName: string;
  maxAttendees?: number;
  attendeeCount: number;
  status: string; // "Draft" | "Published" | "Cancelled"
  organizerId: string;
  organizerName: string;
  createdAt?: string;
}

interface Booking {
  bookingId: string;
  eventId: string;
  title: string;
  description: string;
  location: string;
  date: string;
  organizerName: string;
}

interface Attendee {
  userId: string;
  username: string;
  email: string;
}

interface Comment {
  id: string;
  eventId: string;
  userId: string;
  username: string;
  content: string;
  parentCommentId?: string | null;
  createdAt: string;
}

// The API returns comments as a nested tree (each comment carries its replies).
// We keep them flat in state and rebuild the tree at render time, so SignalR
// events (which arrive as single comments) can be applied uniformly.
const flattenComments = (tree: any[]): Comment[] =>
  tree.flatMap((c) => [
    {
      id: c.id,
      eventId: c.eventId,
      userId: c.userId,
      username: c.username,
      content: c.content,
      parentCommentId: c.parentCommentId ?? null,
      createdAt: c.createdAt,
    },
    ...flattenComments(c.replies || []),
  ]);

// A comment plus every reply beneath it (used when a deletion cascades)
const collectCommentWithDescendants = (rootId: string, all: Comment[]): Set<string> => {
  const ids = new Set([rootId]);
  let changed = true;
  while (changed) {
    changed = false;
    for (const c of all) {
      if (c.parentCommentId && ids.has(c.parentCommentId) && !ids.has(c.id)) {
        ids.add(c.id);
        changed = true;
      }
    }
  }
  return ids;
};

interface UserProfile {
  id: string;
  username: string;
  email: string;
  role: string; // "Admin" | "User"
}

interface AdminUser {
  id: string;
  username: string;
  email: string;
  role: string;
  isBanned: boolean;
  createdAt: string;
}

// 6 High-Fidelity Dummy Events from user picture
const DUMMY_EVENTS: EventItem[] = [
  {
    id: "dummy-1",
    title: "Future Tech Summit 2024",
    description: "Join industry leaders, software engineers, and researchers to discuss future trends in Artificial Intelligence, quantum computing, and decentralized web architectures.",
    location: "San Francisco, CA",
    latitude: 37.7749,
    longitude: -122.4194,
    date: "2026-10-24T10:00:00",
    categoryId: "dummy-cat-tech",
    categoryName: "Technology",
    maxAttendees: 200,
    attendeeCount: 124,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Tech Association"
  },
  {
    id: "dummy-2",
    title: "Mindful Morning Retreat",
    description: "Relax, renew, and find peace. A morning filled with guided meditation, gentle yoga flow, healthy juices, and nature walks in Austin.",
    location: "Austin, TX",
    latitude: 30.2672,
    longitude: -97.7431,
    date: "2026-10-28T08:00:00",
    categoryId: "dummy-cat-wellness",
    categoryName: "Wellness",
    maxAttendees: 50,
    attendeeCount: 15,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Wellness Club"
  },
  {
    id: "dummy-3",
    title: "Urban Canvas Workshop",
    description: "Unleash your creativity. Bring your canvas to life under the guidance of expert street artists. All painting materials provided.",
    location: "New York, NY",
    latitude: 40.7128,
    longitude: -74.0060,
    date: "2026-11-02T13:00:00",
    categoryId: "dummy-cat-arts",
    categoryName: "Art",
    maxAttendees: 30,
    attendeeCount: 22,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Canvas Guild"
  },
  {
    id: "dummy-4",
    title: "Founders & Funders Night",
    description: "Connect with startup founders, venture capitalists, and angel investors in Chicago. Pitch your ideas and discover new partnerships.",
    location: "Chicago, IL",
    latitude: 41.8781,
    longitude: -87.6298,
    date: "2026-11-05T18:30:00",
    categoryId: "dummy-cat-net",
    categoryName: "Networking",
    maxAttendees: 120,
    attendeeCount: 78,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Founders Network"
  },
  {
    id: "dummy-5",
    title: "Product Design Masterclass",
    description: "Learn the secrets of designing products that users love. An intensive hands-on workshop covering UI/UX research, wireframing, and user testing.",
    location: "Seattle, WA",
    latitude: 47.6062,
    longitude: -122.3321,
    date: "2026-11-12T09:00:00",
    categoryId: "dummy-cat-work",
    categoryName: "Workshop",
    maxAttendees: 40,
    attendeeCount: 18,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Design Academy"
  },
  {
    id: "dummy-6",
    title: "Community Arts Festival",
    description: "Celebrate local art, music, food, and culture. A free street festival for families featuring live performances, food trucks, and craft vendors.",
    location: "Portland, OR",
    latitude: 45.5152,
    longitude: -122.6784,
    date: "2026-11-15T11:00:00",
    categoryId: "dummy-cat-arts",
    categoryName: "Art",
    maxAttendees: 1000,
    attendeeCount: 412,
    status: "Published",
    organizerId: "dummy-org",
    organizerName: "Portland Arts Commission"
  }
];

// Fallback dummy categories matching standard filter tags
const DUMMY_CATEGORIES: Category[] = [
  { id: "dummy-cat-tech", name: "Technology", description: "Tech conferences and workshops" },
  { id: "dummy-cat-arts", name: "Art", description: "Arts and creative festivals" },
  { id: "dummy-cat-wellness", name: "Wellness", description: "Health and wellness gatherings" },
  { id: "dummy-cat-net", name: "Networking", description: "Professional networking events" },
  { id: "dummy-cat-work", name: "Workshop", description: "Intensive learning masterclasses" }
];

// Fallback dummy comments for logged-out details preview
const MOCK_COMMENTS: Record<string, Comment[]> = {
  "dummy-1": [
    { id: "mc-1", eventId: "dummy-1", userId: "u-1", username: "Alice", content: "Can't wait for the AI keynotes!", createdAt: new Date(Date.now() - 3600000).toISOString() },
    { id: "mc-2", eventId: "dummy-1", userId: "u-2", username: "Bob", content: "Is there a parking space near the venue?", createdAt: new Date(Date.now() - 1800000).toISOString() }
  ],
  "dummy-2": [
    { id: "mc-3", eventId: "dummy-2", userId: "u-3", username: "Sarah", content: "Should we bring our own yoga mats?", createdAt: new Date().toISOString() }
  ]
};

export default function App() {
  // Navigation & View State
  const [currentView, setCurrentView] = useState<'explore' | 'detail' | 'create' | 'edit' | 'my-events' | 'admin'>('explore');
  const [selectedEventId, setSelectedEventId] = useState<string | null>(null);

  // Authentication State
  const [user, setUser] = useState<UserProfile | null>(() => {
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [authTab, setAuthTab] = useState<'login' | 'register'>('login');

  // Input states for Auth
  const [loginEmail, setLoginEmail] = useState('');
  const [loginPassword, setLoginPassword] = useState('');
  const [registerUsername, setRegisterUsername] = useState('');
  const [registerEmail, setRegisterEmail] = useState('');
  const [registerPassword, setRegisterPassword] = useState('');
  const [registerConfirmPassword, setRegisterConfirmPassword] = useState('');

  // Domain States
  const [categories, setCategories] = useState<Category[]>([]);
  const [events, setEvents] = useState<EventItem[]>([]);
  const [myBookings, setMyBookings] = useState<Booking[]>([]);
  const [myHostedEvents, setMyHostedEvents] = useState<EventItem[]>([]);
  const [likedEvents, setLikedEvents] = useState<Record<string, boolean>>({});

  // Event Detail States
  const [selectedEvent, setSelectedEvent] = useState<EventItem | null>(null);
  const [eventAttendees, setEventAttendees] = useState<Attendee[]>([]);
  const [eventComments, setEventComments] = useState<Comment[]>([]);
  const [newCommentText, setNewCommentText] = useState('');
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [editingCommentText, setEditingCommentText] = useState('');
  const [replyingToCommentId, setReplyingToCommentId] = useState<string | null>(null);
  const [replyText, setReplyText] = useState('');

  // Event Creation/Editing Form State
  const [formTitle, setFormTitle] = useState('');
  const [formDescription, setFormDescription] = useState('');
  const [formLocation, setFormLocation] = useState('');
  const [formLatitude, setFormLatitude] = useState('');
  const [formLongitude, setFormLongitude] = useState('');
  const [formDate, setFormDate] = useState('');
  const [formCategoryId, setFormCategoryId] = useState('');
  const [formMaxAttendees, setFormMaxAttendees] = useState('');

  // Admin Dashboard States
  const [adminTab, setAdminTab] = useState<'categories' | 'users' | 'moderation'>('categories');
  const [adminUsers, setAdminUsers] = useState<AdminUser[]>([]);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newCategoryDesc, setNewCategoryDesc] = useState('');
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
  const [editingCategoryName, setEditingCategoryName] = useState('');
  const [editingCategoryDesc, setEditingCategoryDesc] = useState('');

  // Search, Filter & Pagination
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>('');
  const [activePage, setActivePage] = useState<number>(1);

  // Global Notification/Feedback States
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // SignalR Reference
  const signalRConnRef = useRef<CommentHubConnection | null>(null);

  // Auto-dismiss alerts
  useEffect(() => {
    if (errorMsg) {
      const timer = setTimeout(() => setErrorMsg(null), 6000);
      return () => clearTimeout(timer);
    }
  }, [errorMsg]);

  useEffect(() => {
    if (successMsg) {
      const timer = setTimeout(() => setSuccessMsg(null), 4000);
      return () => clearTimeout(timer);
    }
  }, [successMsg]);

  // Debounce search term — wait 400ms after user stops typing before triggering API call
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearchTerm(searchTerm), 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Initial Boot setup
  useEffect(() => {
    fetchCategories();
    fetchEvents();
    if (token) {
      verifyCurrentUser();
    }
  }, []);

  // Sync token state changes with API and Fetch user properties
  const verifyCurrentUser = async () => {
    const res = await api.me();
    if (res.success && res.data) {
      const profile: UserProfile = res.data;
      setUser(profile);
      localStorage.setItem('user', JSON.stringify(profile));
    } else {
      handleLogout();
    }
  };

  // Fetch Categories and merge with DUMMY_CATEGORIES to match custom layout
  const fetchCategories = async () => {
    const res = await api.getCategories();
    if (res.success && res.data) {
      const dbCategories = res.data;
      const combined = [
        ...dbCategories,
        ...DUMMY_CATEGORIES.filter(dc => !dbCategories.some(dbc => dbc.name.toLowerCase() === dc.name.toLowerCase()))
      ];
      setCategories(combined);
    } else {
      setCategories(DUMMY_CATEGORIES);
    }
  };

  // Fetch Events from API and combine with DUMMY_EVENTS
  const fetchEvents = async () => {
    setLoading(true);
    const res = await api.getEvents(
      selectedCategoryId && !selectedCategoryId.startsWith('dummy') ? selectedCategoryId : undefined,
      debouncedSearchTerm || undefined,
      undefined,
      activePage,
      20
    );
    setLoading(false);
    
    let dbEvents: EventItem[] = [];
    if (res.success && res.data) {
      dbEvents = res.data;
    }

    // Filter out dummy events that have been converted to real DB events (matching title)
    const filteredDummies = DUMMY_EVENTS.filter(de => 
      !dbEvents.some(dbe => dbe.title.toLowerCase() === de.title.toLowerCase())
    );

    // Apply filters locally for dummy events
    let combined = [...dbEvents, ...filteredDummies];
    
    if (selectedCategoryId) {
      combined = combined.filter(e => {
        if (selectedCategoryId.startsWith('dummy-cat-')) {
          const matchCat = DUMMY_CATEGORIES.find(c => c.id === selectedCategoryId);
          return e.categoryName.toLowerCase() === matchCat?.name.toLowerCase();
        }
        return e.categoryId === selectedCategoryId;
      });
    }

    if (debouncedSearchTerm) {
      const query = debouncedSearchTerm.toLowerCase();
      combined = combined.filter(e => 
        e.title.toLowerCase().includes(query) || 
        e.description.toLowerCase().includes(query)
      );
    }

    setEvents(combined);
  };

  // Refresh events list when category, debounced search, or page changes
  useEffect(() => {
    fetchEvents();
  }, [selectedCategoryId, debouncedSearchTerm, activePage]);

  // Fetch My dashboard information
  const fetchMyDashboard = async () => {
    if (!token) return;
    setLoading(true);
    const bookingsRes = await api.getMyBookings();
    const eventsRes = await api.getEvents(); // To filter hosted events locally
    setLoading(false);

    if (bookingsRes.success && bookingsRes.data) {
      setMyBookings(bookingsRes.data);
    }
    if (eventsRes.success && eventsRes.data && user) {
      const hosted = eventsRes.data.filter((e: EventItem) => e.organizerId === user.id);
      setMyHostedEvents(hosted);
    }
  };

  useEffect(() => {
    if (currentView === 'my-events') {
      fetchMyDashboard();
    }
  }, [currentView, token]);

  // Fetch Admin page details
  const fetchAdminData = async () => {
    if (!user || user.role !== 'Admin') return;
    if (adminTab === 'users') {
      setLoading(true);
      const res = await api.adminGetUsers();
      setLoading(false);
      if (res.success && res.data) {
        setAdminUsers(res.data);
      } else {
        setErrorMsg(res.error || 'Failed to retrieve user accounts.');
      }
    }
  };

  useEffect(() => {
    if (currentView === 'admin') {
      fetchAdminData();
    }
  }, [currentView, adminTab]);

  // Real-time SignalR Comment connection configuration
  useEffect(() => {
    if (currentView === 'detail' && selectedEventId && !selectedEventId.startsWith('dummy')) {
      // Connect to comments hub
      const conn = new CommentHubConnection(selectedEventId);
      
      const onCreated = (newComment: any) => {
        setEventComments((prev) => {
          if (prev.some((c) => c.id === newComment.id)) return prev;
          return [...prev, newComment];
        });
      };

      const onUpdated = (updatedComment: any) => {
        // Merge: the update payload only carries id/content/createdAt
        setEventComments((prev) =>
          prev.map((c) => (c.id === updatedComment.id ? { ...c, ...updatedComment } : c))
        );
      };

      const onDeleted = (commentId: string) => {
        setEventComments((prev) => prev.filter((c) => c.id !== commentId));
      };

      conn.start(onCreated, onUpdated, onDeleted);
      signalRConnRef.current = conn;

      return () => {
        if (signalRConnRef.current) {
          signalRConnRef.current.stop();
          signalRConnRef.current = null;
        }
      };
    }
  }, [currentView, selectedEventId]);

  // Fetch Event Details page parameters
  const loadEventDetails = async (id: string) => {
    // If it's a dummy ID, check if a real event with the same title exists in the database
    if (id.startsWith('dummy-')) {
      const dummy = DUMMY_EVENTS.find(de => de.id === id);
      if (dummy) {
        setLoading(true);
        const listRes = await api.getEvents();
        setLoading(false);
        if (listRes.success && listRes.data) {
          const match = listRes.data.find((e: EventItem) => e.title.toLowerCase() === dummy.title.toLowerCase());
          if (match) {
            // Already created in database, load it!
            id = match.id;
          } else if (token) {
            // Logged in: Automatically convert/create dummy event in database behind the scenes so they can join/comment!
            setLoading(true);
            
            // Map dummy category to db category
            let dbCatId = categories.find(c => c.name.toLowerCase() === dummy.categoryName.toLowerCase())?.id;
            if (!dbCatId || dbCatId.startsWith('dummy-cat')) {
              dbCatId = categories.find(c => !c.id.startsWith('dummy-cat'))?.id || categories[0]?.id;
            }

            const createRes = await api.createEvent({
              title: dummy.title,
              description: dummy.description,
              location: dummy.location,
              latitude: dummy.latitude,
              longitude: dummy.longitude,
              date: new Date(dummy.date).toISOString(),
              categoryId: dbCatId,
              maxAttendees: dummy.maxAttendees
            });

            if (createRes.success && createRes.data) {
              const newEvent = createRes.data;
              await api.publishEvent(newEvent.id);
              id = newEvent.id;
            }
            setLoading(false);
          }
        }
      }
    }

    if (id.startsWith('dummy-')) {
      // Still dummy (not logged in fallback or creation failed), load mock details
      const dummy = DUMMY_EVENTS.find(de => de.id === id);
      if (dummy) {
        setSelectedEvent(dummy);
        setSelectedEventId(id);
        setEventAttendees([
          { userId: "mock-att-1", username: "Alice", email: "alice@mock.com" },
          { userId: "mock-att-2", username: "Bob", email: "bob@mock.com" }
        ]);
        setEventComments(MOCK_COMMENTS[dummy.id] || []);
        setCurrentView('detail');
      }
      return;
    }

    // Load actual DB event
    setLoading(true);
    const evRes = await api.getEventById(id);
    const attendeesRes = await api.getEventAttendees(id);
    const commentsRes = await api.getComments(id);
    setLoading(false);

    if (evRes.success && evRes.data) {
      setSelectedEvent(evRes.data);
      setSelectedEventId(id);
      setCurrentView('detail');
    } else {
      setErrorMsg(evRes.error || 'Failed to retrieve event information.');
      return;
    }

    if (attendeesRes.success && attendeesRes.data) {
      setEventAttendees(attendeesRes.data);
    }
    if (commentsRes.success && commentsRes.data) {
      setEventComments(flattenComments(commentsRes.data));
    }
  };

  // Auth: Register
  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (registerPassword !== registerConfirmPassword) {
      setErrorMsg('Passwords do not match.');
      return;
    }
    setLoading(true);
    const res = await api.register({
      username: registerUsername,
      email: registerEmail,
      password: registerPassword,
    });
    setLoading(false);

    if (res.success) {
      setSuccessMsg('Account registered successfully! Please log in.');
      setAuthTab('login');
      // Clear inputs
      setRegisterUsername('');
      setRegisterEmail('');
      setRegisterPassword('');
      setRegisterConfirmPassword('');
    } else {
      setErrorMsg(res.error || 'Registration failed.');
    }
  };

  // Auth: Login
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    const res = await api.login({
      email: loginEmail,
      password: loginPassword,
    });
    setLoading(false);

    if (res.success && res.data) {
      const { token: jwtToken, username, email, id } = res.data;
      localStorage.setItem('token', jwtToken);
      setToken(jwtToken);
      
      // We will perform a verify to fetch the correct user roles
      const profileRes = await api.me();
      if (profileRes.success && profileRes.data) {
        setUser(profileRes.data);
        localStorage.setItem('user', JSON.stringify(profileRes.data));
      } else {
        const fallbackProfile = { id, username, email, role: 'User' };
        setUser(fallbackProfile);
        localStorage.setItem('user', JSON.stringify(fallbackProfile));
      }
      
      setSuccessMsg(`Welcome back, ${username}!`);
      setShowAuthModal(false);
      setLoginEmail('');
      setLoginPassword('');
      fetchEvents();
      // If we were viewing details, re-fetch attendee status
      if (selectedEventId) {
        loadEventDetails(selectedEventId);
      }
    } else {
      setErrorMsg(res.error || 'Invalid credentials or login failed.');
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
    setSuccessMsg('Logged out successfully.');
    setCurrentView('explore');
  };

  // Event: Join Event
  const handleJoinEvent = async (eventId: string) => {
    if (!token) {
      setAuthTab('login');
      setShowAuthModal(true);
      return;
    }
    if (eventId.startsWith('dummy-')) {
      // Convert to DB event on-the-fly when logged in
      await loadEventDetails(eventId);
      return;
    }
    setLoading(true);
    const res = await api.joinEvent(eventId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Successfully joined the event!');
      loadEventDetails(eventId);
    } else {
      setErrorMsg(res.error || 'Failed to join the event.');
    }
  };

  // Event: Leave Event
  const handleLeaveEvent = async (eventId: string) => {
    if (eventId.startsWith('dummy-')) {
      setErrorMsg('This is a demo event.');
      return;
    }
    setLoading(true);
    const res = await api.leaveEvent(eventId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('You have left the event.');
      loadEventDetails(eventId);
    } else {
      setErrorMsg(res.error || 'Failed to leave the event.');
    }
  };

  // Event: Save (Create or Edit)
  const handleSaveEvent = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formTitle || !formDescription || !formLocation || !formDate || !formCategoryId) {
      setErrorMsg('Please complete all required fields.');
      return;
    }

    const payload = {
      title: formTitle,
      description: formDescription,
      location: formLocation,
      latitude: formLatitude ? parseFloat(formLatitude) : null,
      longitude: formLongitude ? parseFloat(formLongitude) : null,
      date: new Date(formDate).toISOString(),
      categoryId: formCategoryId,
      maxAttendees: formMaxAttendees ? parseInt(formMaxAttendees) : null,
    };

    setLoading(true);
    let res;
    if (currentView === 'edit' && selectedEventId) {
      res = await api.updateEvent(selectedEventId, payload);
    } else {
      res = await api.createEvent(payload);
    }
    setLoading(false);

    if (res.success) {
      setSuccessMsg(currentView === 'edit' ? 'Event updated!' : 'Event created as a draft!');
      // Reset form fields
      setFormTitle('');
      setFormDescription('');
      setFormLocation('');
      setFormLatitude('');
      setFormLongitude('');
      setFormDate('');
      setFormCategoryId('');
      setFormMaxAttendees('');

      if (currentView === 'edit') {
        loadEventDetails(selectedEventId!);
      } else {
        setCurrentView('my-events');
      }
    } else {
      setErrorMsg(res.error || 'Failed to save event information.');
    }
  };

  // Edit Event View Pre-population
  const openEditEventView = (ev: EventItem) => {
    if (ev.id.startsWith('dummy')) {
      setErrorMsg('Demo events cannot be edited.');
      return;
    }
    setFormTitle(ev.title);
    setFormDescription(ev.description);
    setFormLocation(ev.location);
    setFormLatitude(ev.latitude ? ev.latitude.toString() : '');
    setFormLongitude(ev.longitude ? ev.longitude.toString() : '');
    // format to YYYY-MM-DDThh:mm
    const dateStr = new Date(ev.date).toISOString().slice(0, 16);
    setFormDate(dateStr);
    setFormCategoryId(ev.categoryId);
    setFormMaxAttendees(ev.maxAttendees ? ev.maxAttendees.toString() : '');
    setSelectedEventId(ev.id);
    setCurrentView('edit');
  };

  // Publish Event
  const handlePublishEvent = async (eventId: string) => {
    setLoading(true);
    const res = await api.publishEvent(eventId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Event published successfully!');
      if (currentView === 'detail') {
        loadEventDetails(eventId);
      } else {
        fetchMyDashboard();
      }
    } else {
      setErrorMsg(res.error || 'Failed to publish event.');
    }
  };

  // Cancel Event
  const handleCancelEvent = async (eventId: string) => {
    if (!confirm('Are you sure you want to cancel this event? Booked users will be notified.')) return;
    setLoading(true);
    const res = await api.cancelEvent(eventId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Event has been cancelled.');
      if (currentView === 'detail') {
        loadEventDetails(eventId);
      } else {
        fetchMyDashboard();
      }
    } else {
      setErrorMsg(res.error || 'Failed to cancel event.');
    }
  };

  // Delete Event (Organizer / Admin)
  const handleDeleteEvent = async (eventId: string, adminForce = false) => {
    if (!confirm('Are you sure you want to delete this event permanently?')) return;
    setLoading(true);
    const res = adminForce ? await api.adminForceDeleteEvent(eventId) : await api.deleteEvent(eventId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Event deleted permanently.');
      if (currentView === 'detail') {
        setCurrentView('explore');
        setSelectedEventId(null);
        fetchEvents();
      } else if (currentView === 'admin') {
        fetchEvents();
      } else {
        fetchMyDashboard();
      }
    } else {
      setErrorMsg(res.error || 'Failed to delete event.');
    }
  };

  // Comments CRUD
  const handlePostComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCommentText.trim() || !selectedEventId) return;

    if (selectedEventId.startsWith('dummy-')) {
      // Demo comment addition locally
      const newCom: Comment = {
        id: `mock-com-${Date.now()}`,
        eventId: selectedEventId,
        userId: user?.id || 'mock-user',
        username: user?.username || 'Guest',
        content: newCommentText,
        createdAt: new Date().toISOString()
      };
      setEventComments(prev => [...prev, newCom]);
      setNewCommentText('');
      return;
    }

    setLoading(true);
    const res = await api.createComment(selectedEventId, { content: newCommentText });
    setLoading(false);
    if (res.success) {
      setNewCommentText('');
    } else {
      setErrorMsg(res.error || 'Failed to submit comment.');
    }
  };

  const handlePostReply = async (parentCommentId: string) => {
    if (!replyText.trim() || !selectedEventId) return;

    if (selectedEventId.startsWith('dummy-') || parentCommentId.startsWith('mock-com-')) {
      // Demo reply addition locally
      const newCom: Comment = {
        id: `mock-com-${Date.now()}`,
        eventId: selectedEventId,
        userId: user?.id || 'mock-user',
        username: user?.username || 'Guest',
        content: replyText,
        parentCommentId,
        createdAt: new Date().toISOString()
      };
      setEventComments(prev => [...prev, newCom]);
      setReplyingToCommentId(null);
      setReplyText('');
      return;
    }

    setLoading(true);
    const res = await api.createComment(selectedEventId, { content: replyText, parentCommentId });
    setLoading(false);
    if (res.success) {
      setReplyingToCommentId(null);
      setReplyText('');
    } else {
      setErrorMsg(res.error || 'Failed to submit reply.');
    }
  };

  const handleUpdateComment = async (commentId: string) => {
    if (!editingCommentText.trim() || !selectedEventId) return;

    if (commentId.startsWith('mock-com-')) {
      setEventComments(prev => prev.map(c => c.id === commentId ? { ...c, content: editingCommentText } : c));
      setEditingCommentId(null);
      setEditingCommentText('');
      return;
    }

    setLoading(true);
    const res = await api.updateComment(selectedEventId, commentId, { content: editingCommentText });
    setLoading(false);
    if (res.success) {
      setEditingCommentId(null);
      setEditingCommentText('');
    } else {
      setErrorMsg(res.error || 'Failed to update comment.');
    }
  };

  const handleDeleteComment = async (commentId: string, adminForce = false) => {
    if (!confirm('Delete this comment permanently? Replies to it will be deleted too.')) return;

    if (commentId.startsWith('mock-com-')) {
      setEventComments(prev => {
        const removed = collectCommentWithDescendants(commentId, prev);
        return prev.filter(c => !removed.has(c.id));
      });
      return;
    }

    if (!selectedEventId) return;

    setLoading(true);
    const res = adminForce ? await api.adminForceDeleteComment(commentId) : await api.deleteComment(selectedEventId, commentId);
    setLoading(false);
    if (res.success) {
      // Deleting a comment also removes its replies (the backend cascades)
      setEventComments(prev => {
        const removed = collectCommentWithDescendants(commentId, prev);
        return prev.filter(c => !removed.has(c.id));
      });
    } else {
      setErrorMsg(res.error || 'Failed to delete comment.');
    }
  };

  // Admin: Create Category
  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategoryName.trim()) return;
    setLoading(true);
    const res = await api.adminCreateCategory({
      name: newCategoryName,
      description: newCategoryDesc,
    });
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Category created.');
      setNewCategoryName('');
      setNewCategoryDesc('');
      fetchCategories();
    } else {
      setErrorMsg(res.error || 'Failed to create category.');
    }
  };

  // Admin: Edit Category
  const handleUpdateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingCategoryId || !editingCategoryName.trim()) return;
    setLoading(true);
    const res = await api.adminUpdateCategory(editingCategoryId, {
      name: editingCategoryName,
      description: editingCategoryDesc,
    });
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Category updated.');
      setEditingCategoryId(null);
      setEditingCategoryName('');
      setEditingCategoryDesc('');
      fetchCategories();
    } else {
      setErrorMsg(res.error || 'Failed to update category.');
    }
  };

  // Admin: Delete Category
  const handleDeleteCategory = async (id: string) => {
    if (!confirm('Are you sure you want to delete this category?')) return;
    setLoading(true);
    const res = await api.adminDeleteCategory(id);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('Category deleted.');
      fetchCategories();
    } else {
      setErrorMsg(res.error || 'Failed to delete category.');
    }
  };

  // Admin: Ban User
  const handleBanUser = async (userId: string) => {
    if (!confirm('Are you sure you want to ban this user?')) return;
    setLoading(true);
    const res = await api.adminBanUser(userId);
    setLoading(false);
    if (res.success) {
      setSuccessMsg('User banned successfully.');
      fetchAdminData();
    } else {
      setErrorMsg(res.error || 'Failed to ban user.');
    }
  };

  const toggleLike = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setLikedEvents(prev => ({ ...prev, [id]: !prev[id] }));
  };

  const handleCreateEventClick = () => {
    if (!token) {
      setAuthTab('login');
      setShowAuthModal(true);
      return;
    }
    setFormTitle('');
    setFormDescription('');
    setFormLocation('');
    setFormLatitude('');
    setFormLongitude('');
    setFormDate('');
    setFormCategoryId(categories[0]?.id || '');
    setFormMaxAttendees('');
    setCurrentView('create');
  };

  const handleNavigate = (view: 'explore' | 'my-events' | 'admin') => {
    setCurrentView(view);
    setSelectedEventId(null);
    setSelectedEvent(null);
    if (view === 'explore') {
      fetchEvents();
    }
  };

  const getCategoryMeta = (catName: string) => {
    const name = catName?.toLowerCase() || '';
    if (name.includes('tech')) return { badgeClass: 'bg-[#4648d4]/10 text-[#4648d4]', image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600&auto=format&fit=crop&q=80' };
    if (name.includes('sport') || name.includes('wellness')) return { badgeClass: 'bg-emerald-100 text-emerald-800', image: 'https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=600&auto=format&fit=crop&q=80' };
    if (name.includes('music')) return { badgeClass: 'bg-pink-100 text-pink-700', image: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=600&auto=format&fit=crop&q=80' };
    if (name.includes('art')) return { badgeClass: 'bg-orange-100 text-orange-800', image: 'https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=600&auto=format&fit=crop&q=80' };
    return { badgeClass: 'bg-slate-100 text-slate-700', image: 'https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=600&auto=format&fit=crop&q=80' };
  };

  return (
    <div className="min-h-screen flex flex-col bg-[#f8f9ff] text-slate-800 font-body-md select-none">
      
      {/* Global Alerts */}
      {errorMsg && (
        <div className="fixed top-4 right-4 z-50 flex items-center gap-3 p-4 bg-red-50 text-red-800 border border-red-200 rounded-2xl shadow-xl max-w-md animate-pulse-subtle">
          <span className="material-symbols-outlined text-red-600">error</span>
          <p className="text-xs font-bold">{errorMsg}</p>
          <button onClick={() => setErrorMsg(null)} className="ml-auto flex items-center hover:opacity-75">
            <span className="material-symbols-outlined text-lg">close</span>
          </button>
        </div>
      )}

      {successMsg && (
        <div className="fixed top-4 right-4 z-50 flex items-center gap-3 p-4 bg-green-50 text-green-800 border border-green-200 rounded-2xl shadow-xl max-w-md">
          <span className="material-symbols-outlined text-green-600">check_circle</span>
          <p className="text-xs font-bold">{successMsg}</p>
          <button onClick={() => setSuccessMsg(null)} className="ml-auto flex items-center hover:opacity-75">
            <span className="material-symbols-outlined text-lg">close</span>
          </button>
        </div>
      )}

      {/* Global Loader overlay */}
      {loading && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-white/50 backdrop-blur-xs">
          <div className="flex flex-col items-center gap-2 p-5 rounded-2xl bg-white shadow-xl border border-slate-100">
            <div className="w-8 h-8 border-[3px] border-[#4648d4] border-t-transparent rounded-full animate-spin"></div>
            <p className="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-1">Loading...</p>
          </div>
        </div>
      )}

      {/* Componentized Navbar */}
      <Navbar
        user={user}
        token={token}
        onLogout={handleLogout}
        onShowAuth={(tab) => { setAuthTab(tab); setShowAuthModal(true); }}
        currentView={currentView}
        onNavigate={handleNavigate}
        searchTerm={searchTerm}
        onSearchChange={setSearchTerm}
        onCreateEventClick={handleCreateEventClick}
      />

      {/* Main Container */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-6 md:p-8">
        
        {/* VIEW: EXPLORE HUB */}
        {currentView === 'explore' && (
          <div className="space-y-8 animate-fade-in">
            {/* Componentized Hero */}
            <Hero />

            {/* Componentized Category pills */}
            <CategoryFilters
              categories={categories}
              selectedCategoryId={selectedCategoryId}
              onSelectCategory={setSelectedCategoryId}
            />

            {/* Event Grid */}
            <div className="space-y-6">
              {events.filter(e => e.status === 'Published').length === 0 ? (
                <div className="text-center py-20 bg-white border border-slate-200 rounded-2xl shadow-xs space-y-4">
                  <span className="material-symbols-outlined text-4xl text-slate-300">event_busy</span>
                  <div className="space-y-1">
                    <h4 className="font-extrabold text-slate-700 text-sm">No events found</h4>
                    <p className="text-xs text-slate-400 max-w-sm mx-auto">Try resetting filters or search terms.</p>
                  </div>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {events
                    .filter((e) => e.status === 'Published')
                    .map((ev) => (
                      <EventCard
                        key={ev.id}
                        ev={ev}
                        isLiked={likedEvents[ev.id] || false}
                        onToggleLike={toggleLike}
                        onViewDetails={loadEventDetails}
                      />
                    ))}
                </div>
              )}
            </div>

            {/* Componentized Pagination */}
            {events.filter(e => e.status === 'Published').length > 0 && (
              <Pagination
                activePage={activePage}
                onPageChange={setActivePage}
              />
            )}
          </div>
        )}

        {/* VIEW: EVENT DETAILS */}
        {currentView === 'detail' && selectedEvent && (
          <div className="space-y-6 animate-fade-in text-left">
            <button
              onClick={() => handleNavigate('explore')}
              className="inline-flex items-center gap-1.5 text-xs font-bold text-slate-400 hover:text-[#4648d4] transition-colors"
            >
              <span className="material-symbols-outlined text-lg">arrow_back</span>
              Back to Explore
            </button>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              
              {/* Event Body */}
              <div className="lg:col-span-2 space-y-6">
                
                <div className="rounded-2xl bg-white border border-slate-200 overflow-hidden shadow-xs">
                  <div className="h-60 relative bg-slate-100">
                    <img 
                      src={getCategoryMeta(selectedEvent.categoryName).image} 
                      alt={selectedEvent.title} 
                      className="w-full h-full object-cover" 
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent"></div>
                    <div className="absolute bottom-6 left-6 text-white space-y-2">
                      <span className={`px-2 py-0.5 rounded text-[9px] font-extrabold uppercase bg-white/20 backdrop-blur-md text-white border border-white/10`}>
                        {selectedEvent.categoryName}
                      </span>
                      <h2 className="text-xl md:text-2xl font-extrabold tracking-tight">
                        {selectedEvent.title}
                      </h2>
                    </div>
                  </div>
                  
                  <div className="p-5 flex flex-wrap items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/50">
                    <div className="flex items-center gap-3 text-xs font-bold text-slate-400">
                      <span className="flex items-center gap-1">
                        <span className="material-symbols-outlined text-sm text-[#4648d4]">person</span>
                        Host: {selectedEvent.organizerName}
                      </span>
                      <span className="w-1 h-1 rounded-full bg-slate-300"></span>
                      <span className="flex items-center gap-1">
                        <span className="material-symbols-outlined text-sm text-[#4648d4]">security</span>
                        Status: {selectedEvent.status}
                      </span>
                    </div>

                    {user && (user.id === selectedEvent.organizerId || user.role === 'Admin') && (
                      <div className="flex gap-2">
                        {selectedEvent.status === 'Draft' && (
                          <button
                            onClick={() => handlePublishEvent(selectedEvent.id)}
                            className="px-3 py-1.5 bg-emerald-500 hover:bg-emerald-600 text-white rounded-lg text-xs font-bold transition-colors shadow-sm"
                          >
                            Publish
                          </button>
                        )}
                        {selectedEvent.status === 'Published' && (
                          <button
                            onClick={() => handleCancelEvent(selectedEvent.id)}
                            className="px-3 py-1.5 bg-amber-500 hover:bg-amber-600 text-white rounded-lg text-xs font-bold transition-colors shadow-sm"
                          >
                            Cancel
                          </button>
                        )}
                        <button
                          onClick={() => openEditEventView(selectedEvent)}
                          className="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 border border-slate-200 rounded-lg text-xs font-bold transition-colors"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDeleteEvent(selectedEvent.id, user.role === 'Admin')}
                          className="px-3 py-1.5 bg-red-50 hover:bg-red-100 text-red-600 rounded-lg text-xs font-bold transition-colors"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </div>
                </div>

                <div className="bg-white rounded-2xl p-6 border border-slate-200 space-y-3 shadow-xs">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider">About Gathering</h3>
                  <p className="text-slate-500 text-xs leading-relaxed font-semibold whitespace-pre-wrap">
                    {selectedEvent.description}
                  </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="bg-white rounded-2xl p-5 border border-slate-200 flex gap-3.5 items-start shadow-xs">
                    <div className="w-9 h-9 rounded-xl bg-[#4648d4]/10 text-[#4648d4] flex items-center justify-center shrink-0">
                      <span className="material-symbols-outlined text-lg">calendar_today</span>
                    </div>
                    <div>
                      <h4 className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Date & Time</h4>
                      <p className="text-xs font-extrabold text-slate-800 mt-1">
                        {new Date(selectedEvent.date).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}
                      </p>
                      <p className="text-[10px] text-slate-500 font-bold mt-0.5">
                        {new Date(selectedEvent.date).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })}
                      </p>
                    </div>
                  </div>

                  <div className="bg-white rounded-2xl p-5 border border-slate-200 flex gap-3.5 items-start shadow-xs">
                    <div className="w-9 h-9 rounded-xl bg-[#4648d4]/10 text-[#4648d4] flex items-center justify-center shrink-0">
                      <span className="material-symbols-outlined text-lg">location_on</span>
                    </div>
                    <div className="flex-1">
                      <h4 className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Venue Location</h4>
                      <p className="text-xs font-extrabold text-slate-800 mt-1 truncate">
                        {selectedEvent.location}
                      </p>
                      {selectedEvent.latitude && selectedEvent.longitude && (
                        <p className="text-[9px] text-[#4648d4] font-bold mt-1 tracking-wider uppercase">
                          GPS Coords: {selectedEvent.latitude.toFixed(4)}, {selectedEvent.longitude.toFixed(4)}
                        </p>
                      )}
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-2xl p-6 border border-slate-200 space-y-4 shadow-xs">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider">Attendees</h3>
                      <p className="text-[10px] text-slate-400 font-semibold mt-0.5">Community attendee listings</p>
                    </div>
                    <span className="px-2.5 py-0.5 bg-slate-50 border border-slate-100 rounded-full text-xs font-bold">
                      {eventAttendees.length} Joined
                    </span>
                  </div>

                  {selectedEvent.maxAttendees && (
                    <div className="space-y-1.5">
                      <div className="flex items-center justify-between text-xs font-bold text-slate-400">
                        <span>Capacity Occupied</span>
                        <span>{Math.max(0, selectedEvent.maxAttendees - eventAttendees.length)} spots available</span>
                      </div>
                      <div className="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
                        <div 
                          className="h-full bg-gradient-to-r from-[#4648d4] to-indigo-400 rounded-full"
                          style={{ width: `${Math.min(100, (eventAttendees.length / selectedEvent.maxAttendees) * 100)}%` }}
                        ></div>
                      </div>
                    </div>
                  )}

                  <div className="pt-2 flex items-center justify-between gap-4 border-t border-slate-100">
                    {token ? (
                      eventAttendees.some(a => a.userId === user?.id) ? (
                        <button
                          onClick={() => handleLeaveEvent(selectedEvent.id)}
                          className="px-4 py-2 bg-red-50 text-red-600 hover:bg-red-100 font-bold rounded-xl active:scale-95 transition-all text-xs flex items-center gap-1.5 shadow-sm"
                        >
                          <span className="material-symbols-outlined text-base">logout</span>
                          Leave Event
                        </button>
                      ) : (
                        <button
                          onClick={() => handleJoinEvent(selectedEvent.id)}
                          disabled={selectedEvent.maxAttendees !== undefined && eventAttendees.length >= selectedEvent.maxAttendees}
                          className="px-4 py-2 bg-[#4648d4] hover:bg-[#3738bd] text-white font-bold rounded-xl active:scale-95 transition-all text-xs disabled:opacity-55 disabled:pointer-events-none flex items-center gap-1.5 shadow-sm"
                        >
                          <span className="material-symbols-outlined text-base">check_circle</span>
                          {selectedEvent.maxAttendees !== undefined && eventAttendees.length >= selectedEvent.maxAttendees ? 'Event Full' : 'Join Event'}
                        </button>
                      )
                    ) : (
                      <div className="w-full flex flex-wrap items-center justify-between gap-3 bg-slate-50 p-4 rounded-xl border border-slate-200">
                        <div>
                          <p className="text-xs font-bold text-slate-700">Want to attend this event?</p>
                          <p className="text-[10px] text-slate-400 mt-0.5">Please sign in to book your ticket spot.</p>
                        </div>
                        <button
                          onClick={() => { setAuthTab('login'); setShowAuthModal(true); }}
                          className="px-3.5 py-1.5 bg-[#4648d4] text-white text-xs font-bold rounded-xl hover:bg-[#3738bd] transition-colors"
                        >
                          Sign In
                        </button>
                      </div>
                    )}
                  </div>

                  {eventAttendees.length === 0 ? (
                    <p className="text-xs text-slate-400 italic text-center py-2">No users signed up yet.</p>
                  ) : (
                    <div className="flex flex-wrap gap-2 pt-2">
                      {eventAttendees.map((att) => (
                        <span 
                          key={att.userId}
                          className="px-3 py-1.5 bg-slate-50 text-slate-600 text-xs font-bold rounded-xl border border-slate-100 flex items-center gap-1.5"
                        >
                          <div className="w-4 h-4 rounded-full bg-[#4648d4]/10 text-[#4648d4] flex items-center justify-center text-[10px] font-bold">
                            {att.username.charAt(0).toUpperCase()}
                          </div>
                          {att.username}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              {/* Chat Board */}
              <div className="bg-white rounded-2xl border border-slate-200 shadow-xs overflow-hidden flex flex-col h-[520px] lg:h-auto">
                <div className="p-4 border-b border-slate-100 bg-slate-50 flex items-center justify-between">
                  <h3 className="text-[10px] font-extrabold text-slate-800 uppercase tracking-wider flex items-center gap-1">
                    <span className="w-1.5 h-1.5 rounded-full bg-red-500 animate-ping"></span>
                    Live Conversations
                  </h3>
                  <span className="px-2 py-0.5 bg-red-50 text-red-700 text-[8px] font-bold rounded-full uppercase tracking-wider">
                    SignalR Live
                  </span>
                </div>

                <div className="flex-1 overflow-y-auto p-4 space-y-4">
                  {eventComments.length === 0 ? (
                    <div className="h-full flex flex-col items-center justify-center text-center text-slate-400 space-y-2">
                      <span className="material-symbols-outlined text-xl opacity-60">forum</span>
                      <p className="text-xs font-bold">No chat history.</p>
                      <p className="text-[10px] max-w-[150px]">Coordinate with other attendees here live!</p>
                    </div>
                  ) : (
                    (() => {
                      const renderComment = (com: Comment, depth: number): React.ReactNode => {
                        const isOwner = user && com.userId === user.id;
                        const isAdmin = user?.role === 'Admin';
                        const replies = eventComments
                          .filter((c) => c.parentCommentId === com.id)
                          .sort((a, b) => a.createdAt.localeCompare(b.createdAt));
                        return (
                          <div key={com.id} className="group/com space-y-1">
                            <div className="flex items-center justify-between">
                              <div className="flex items-center gap-1.5">
                                <span className="text-[9px] font-extrabold text-slate-700">
                                  {com.username}
                                </span>
                                {selectedEvent.organizerId === com.userId && (
                                  <span className="px-1 py-0.2 bg-[#4648d4]/10 text-[#4648d4] text-[8px] font-extrabold rounded uppercase tracking-wider">
                                    Host
                                  </span>
                                )}
                                <span className="text-[9px] text-slate-400 font-bold">
                                  {new Date(com.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                </span>
                              </div>

                              {token && (
                                <div className="opacity-0 group-hover/com:opacity-100 flex items-center gap-1 transition-opacity">
                                  <button
                                    onClick={() => { setReplyingToCommentId(replyingToCommentId === com.id ? null : com.id); setReplyText(''); }}
                                    className="p-0.5 hover:text-[#4648d4] text-slate-400 transition-colors"
                                    title="Reply"
                                  >
                                    <span className="material-symbols-outlined text-sm font-bold">reply</span>
                                  </button>
                                  {isOwner && editingCommentId !== com.id && (
                                    <button
                                      onClick={() => { setEditingCommentId(com.id); setEditingCommentText(com.content); }}
                                      className="p-0.5 hover:text-[#4648d4] text-slate-400 transition-colors"
                                    >
                                      <span className="material-symbols-outlined text-sm font-bold">edit</span>
                                    </button>
                                  )}
                                  {(isOwner || isAdmin) && (
                                    <button
                                      onClick={() => handleDeleteComment(com.id, isAdmin && !isOwner)}
                                      className="p-0.5 hover:text-red-500 text-slate-400 transition-colors"
                                    >
                                      <span className="material-symbols-outlined text-sm font-bold">delete</span>
                                    </button>
                                  )}
                                </div>
                              )}
                            </div>

                            {editingCommentId === com.id ? (
                              <div className="space-y-1.5 bg-slate-50 p-2 rounded-xl border border-[#4648d4]/20">
                                <textarea
                                  value={editingCommentText}
                                  onChange={(e) => setEditingCommentText(e.target.value)}
                                  rows={2}
                                  className="w-full text-xs p-1.5 rounded bg-white text-slate-800 border border-slate-200 focus:outline-none focus:ring-1 focus:ring-[#4648d4]"
                                />
                                <div className="flex items-center justify-end gap-1">
                                  <button
                                    onClick={() => setEditingCommentId(null)}
                                    className="px-2 py-1 bg-slate-200 text-slate-600 text-[8px] font-bold rounded"
                                  >
                                    Cancel
                                  </button>
                                  <button
                                    onClick={() => handleUpdateComment(com.id)}
                                    className="px-2 py-1 bg-[#4648d4] text-white text-[8px] font-bold rounded"
                                  >
                                    Save
                                  </button>
                                </div>
                              </div>
                            ) : (
                              <div className="bg-slate-50 p-2.5 rounded-2xl text-xs text-slate-600 font-medium leading-relaxed border border-slate-100/50 whitespace-pre-wrap">
                                {com.content}
                              </div>
                            )}

                            {replyingToCommentId === com.id && (
                              <div className="space-y-1.5 bg-slate-50 p-2 rounded-xl border border-[#4648d4]/20">
                                <textarea
                                  value={replyText}
                                  onChange={(e) => setReplyText(e.target.value)}
                                  placeholder={`Reply to ${com.username}...`}
                                  rows={2}
                                  className="w-full text-xs p-1.5 rounded bg-white text-slate-800 border border-slate-200 focus:outline-none focus:ring-1 focus:ring-[#4648d4]"
                                />
                                <div className="flex items-center justify-end gap-1">
                                  <button
                                    onClick={() => { setReplyingToCommentId(null); setReplyText(''); }}
                                    className="px-2 py-1 bg-slate-200 text-slate-600 text-[8px] font-bold rounded"
                                  >
                                    Cancel
                                  </button>
                                  <button
                                    onClick={() => handlePostReply(com.id)}
                                    disabled={!replyText.trim()}
                                    className="px-2 py-1 bg-[#4648d4] text-white text-[8px] font-bold rounded disabled:opacity-40"
                                  >
                                    Reply
                                  </button>
                                </div>
                              </div>
                            )}

                            {replies.length > 0 && (
                              <div className="ml-3 pl-3 border-l-2 border-slate-100 space-y-3 pt-1">
                                {replies.map((reply) => renderComment(reply, depth + 1))}
                              </div>
                            )}
                          </div>
                        );
                      };

                      return eventComments
                        .filter((c) => !c.parentCommentId || !eventComments.some((p) => p.id === c.parentCommentId))
                        .sort((a, b) => a.createdAt.localeCompare(b.createdAt))
                        .map((com) => renderComment(com, 0));
                    })()
                  )}
                </div>

                <div className="p-3 border-t border-slate-100 bg-white">
                  {token ? (
                    <form onSubmit={handlePostComment} className="flex gap-2 items-end">
                      <textarea
                        value={newCommentText}
                        onChange={(e) => setNewCommentText(e.target.value)}
                        placeholder="Write comments..."
                        rows={2}
                        className="flex-1 text-xs p-2 bg-slate-50 text-slate-800 border border-slate-100 rounded-xl focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] placeholder:text-slate-400 resize-none"
                      />
                      <button
                        type="submit"
                        disabled={!newCommentText.trim()}
                        className="h-9 w-9 rounded-xl bg-[#4648d4] text-white flex items-center justify-center shrink-0 disabled:opacity-40 disabled:pointer-events-none transition-all shadow-sm"
                      >
                        <span className="material-symbols-outlined text-base">send</span>
                      </button>
                    </form>
                  ) : (
                    <div className="text-center py-1.5">
                      <button
                        onClick={() => { setAuthTab('login'); setShowAuthModal(true); }}
                        className="text-[10px] text-[#4648d4] font-bold underline"
                      >
                        Log in to participate
                      </button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* VIEW: ORGANIZE EVENT / CREATE / EDIT */}
        {(currentView === 'create' || currentView === 'edit') && (
          <div className="max-w-2xl mx-auto space-y-6 animate-fade-in text-left">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-xl font-extrabold tracking-tight text-slate-900">
                  {currentView === 'edit' ? 'Update Gathering Details' : 'Organize New Experience'}
                </h2>
                <p className="text-xs text-slate-400 font-semibold mt-0.5">
                  Define event parameters and host community conversations.
                </p>
              </div>
              <button
                onClick={() => {
                  if (currentView === 'edit' && selectedEventId) {
                    loadEventDetails(selectedEventId);
                  } else {
                    setCurrentView('explore');
                  }
                }}
                className="w-8 h-8 rounded-xl bg-slate-100 hover:bg-slate-200 flex items-center justify-center text-slate-400 hover:text-slate-700 transition-colors"
              >
                <span className="material-symbols-outlined text-lg">close</span>
              </button>
            </div>

            <form onSubmit={handleSaveEvent} className="bg-white rounded-2xl p-6 border border-slate-200 shadow-md space-y-6">
              <div className="space-y-1.5">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Event Title *</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. GatherPulse Hackathon 2026"
                  value={formTitle}
                  onChange={(e) => setFormTitle(e.target.value)}
                  className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                />
              </div>

              <div className="space-y-1.5">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Category Category *</label>
                <select
                  required
                  value={formCategoryId}
                  onChange={(e) => setFormCategoryId(e.target.value)}
                  className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                >
                  <option value="" disabled>Choose Category</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>

              <div className="space-y-1.5">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Gathering Description *</label>
                <textarea
                  required
                  rows={4}
                  placeholder="Describe your event content, itinerary, guidelines..."
                  value={formDescription}
                  onChange={(e) => setFormDescription(e.target.value)}
                  className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Date & Time *</label>
                  <input
                    type="datetime-local"
                    required
                    value={formDate}
                    onChange={(e) => setFormDate(e.target.value)}
                    className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                  />
                </div>

                <div className="space-y-1.5">
                  <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Max Attendees Limit</label>
                  <input
                    type="number"
                    min="1"
                    placeholder="Leave blank for unlimited"
                    value={formMaxAttendees}
                    onChange={(e) => setFormMaxAttendees(e.target.value)}
                    className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Location Venue Address *</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. 100 Main St, San Francisco, CA"
                  value={formLocation}
                  onChange={(e) => setFormLocation(e.target.value)}
                  className="w-full p-3 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] transition-all text-xs"
                />
              </div>

              <div className="p-4 bg-slate-50 rounded-2xl border border-slate-100 space-y-3">
                <div className="flex items-center justify-between border-b border-slate-200 pb-2">
                  <h4 className="text-xs font-bold text-slate-700">Geomap Coordinates (Optional)</h4>
                  <span className="px-2 py-0.5 rounded bg-[#4648d4]/10 text-[#4648d4] text-[8px] font-extrabold uppercase">Coords</span>
                </div>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Latitude</label>
                    <input
                      type="number"
                      step="any"
                      placeholder="e.g. 37.7749"
                      value={formLatitude}
                      onChange={(e) => setFormLatitude(e.target.value)}
                      className="w-full p-2.5 bg-white text-slate-800 rounded-xl border border-slate-200 focus:outline-none focus:ring-1 focus:ring-[#4648d4] text-xs"
                    />
                  </div>

                  <div className="space-y-1.5">
                    <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Longitude</label>
                    <input
                      type="number"
                      step="any"
                      placeholder="e.g. -122.4194"
                      value={formLongitude}
                      onChange={(e) => setFormLongitude(e.target.value)}
                      className="w-full p-2.5 bg-white text-slate-800 rounded-xl border border-slate-200 focus:outline-none focus:ring-1 focus:ring-[#4648d4] text-xs"
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => {
                    if (currentView === 'edit' && selectedEventId) {
                      loadEventDetails(selectedEventId);
                    } else {
                      setCurrentView('explore');
                    }
                  }}
                  className="px-4 py-2 bg-slate-100 text-slate-600 text-xs font-bold rounded-xl hover:bg-slate-200 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4.5 py-2 bg-[#4648d4] hover:bg-[#3738bd] text-white text-xs font-bold rounded-xl transition-all shadow-sm"
                >
                  {currentView === 'edit' ? 'Save Changes' : 'Create Draft'}
                </button>
              </div>
            </form>
          </div>
        )}

        {/* VIEW: MY DASHBOARD EVENTS */}
        {currentView === 'my-events' && (
          <div className="space-y-6 animate-fade-in text-left">
            <div>
              <h2 className="text-xl font-extrabold tracking-tight text-slate-900">My Event Dashboard</h2>
              <p className="text-xs text-slate-400 font-semibold mt-0.5">Manage events you organized or joined.</p>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              
              <div className="bg-white rounded-2xl p-6 border border-slate-200 space-y-4 shadow-xs">
                <div className="flex items-center justify-between border-b border-slate-100 pb-3">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider flex items-center gap-2">
                    <span className="material-symbols-outlined text-[#4648d4] text-lg">campaign</span>
                    My Hosted Gatherings
                  </h3>
                  <span className="px-2 py-0.5 rounded-full bg-[#4648d4]/10 text-[#4648d4] text-[9px] font-bold">
                    {myHostedEvents.length} Hosted
                  </span>
                </div>

                {myHostedEvents.length === 0 ? (
                  <div className="text-center py-12 text-slate-400 space-y-2">
                    <span className="material-symbols-outlined text-2xl opacity-60">post_add</span>
                    <p className="text-xs font-bold">No organized events.</p>
                    <button
                      onClick={handleCreateEventClick}
                      className="text-xs text-[#4648d4] font-bold underline"
                    >
                      Host event now
                    </button>
                  </div>
                ) : (
                  <div className="space-y-3 max-h-[450px] overflow-y-auto pr-1">
                    {myHostedEvents.map((ev) => (
                      <div 
                        key={ev.id}
                        className="p-4 rounded-2xl bg-[#f8f9ff] border border-slate-100 flex items-center justify-between gap-4"
                      >
                        <div className="space-y-1 flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <span className="text-[8px] font-extrabold uppercase bg-white px-2 py-0.5 rounded border border-slate-200 text-slate-500">
                              {ev.categoryName}
                            </span>
                            {ev.status === 'Draft' && <span className="text-[8px] font-bold text-amber-600 bg-amber-50 px-1.5 py-0.2 rounded border border-amber-100">Draft</span>}
                            {ev.status === 'Published' && <span className="text-[8px] font-bold text-emerald-600 bg-emerald-50 px-1.5 py-0.2 rounded border border-emerald-100">Live</span>}
                            {ev.status === 'Cancelled' && <span className="text-[8px] font-bold text-red-600 bg-red-50 px-1.5 py-0.2 rounded border border-red-100">Cancelled</span>}
                          </div>
                          <h4 className="text-xs font-extrabold text-slate-800 truncate hover:text-[#4648d4] cursor-pointer" onClick={() => loadEventDetails(ev.id)}>
                            {ev.title}
                          </h4>
                          <p className="text-[9px] text-slate-400 font-bold">
                            {new Date(ev.date).toLocaleDateString()} at {new Date(ev.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </p>
                        </div>

                        <div className="flex items-center gap-1.5 shrink-0">
                          {ev.status === 'Draft' && (
                            <button
                              onClick={() => handlePublishEvent(ev.id)}
                              className="px-2 py-1 bg-emerald-500 hover:bg-emerald-600 text-white font-bold text-[9px] rounded shadow-sm animate-pulse-subtle"
                            >
                              Publish
                            </button>
                          )}
                          {ev.status === 'Published' && (
                            <button
                              onClick={() => handleCancelEvent(ev.id)}
                              className="px-2 py-1 bg-amber-500 hover:bg-amber-600 text-white font-bold text-[9px] rounded shadow-sm"
                            >
                              Cancel
                            </button>
                          )}
                          <button
                            onClick={() => openEditEventView(ev)}
                            className="w-7 h-7 rounded bg-white border border-slate-200 flex items-center justify-center text-slate-400 hover:text-slate-700"
                          >
                            <span className="material-symbols-outlined text-base">edit</span>
                          </button>
                          <button
                            onClick={() => handleDeleteEvent(ev.id)}
                            className="w-7 h-7 rounded bg-red-50 text-red-600 flex items-center justify-center hover:bg-red-100"
                          >
                            <span className="material-symbols-outlined text-base">delete</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div className="bg-white rounded-2xl p-6 border border-slate-200 space-y-4 shadow-xs">
                <div className="flex items-center justify-between border-b border-slate-100 pb-3">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider flex items-center gap-2">
                    <span className="material-symbols-outlined text-[#4648d4] text-lg">local_activity</span>
                    My Booked Tickets
                  </h3>
                  <span className="px-2 py-0.5 rounded-full bg-[#4648d4]/10 text-[#4648d4] text-[9px] font-bold">
                    {myBookings.length} Booked
                  </span>
                </div>

                {myBookings.length === 0 ? (
                  <div className="text-center py-12 text-slate-400 space-y-2">
                    <span className="material-symbols-outlined text-2xl opacity-60">confirmation_number</span>
                    <p className="text-xs font-bold">No booked tickets.</p>
                    <button
                      onClick={() => setCurrentView('explore')}
                      className="text-xs text-[#4648d4] font-bold underline"
                    >
                      Find experiences
                    </button>
                  </div>
                ) : (
                  <div className="space-y-3 max-h-[450px] overflow-y-auto pr-1">
                    {myBookings.map((b) => (
                      <div 
                        key={b.bookingId}
                        className="p-4 rounded-2xl bg-[#f8f9ff] border border-slate-100 flex items-center justify-between gap-4"
                      >
                        <div className="space-y-1 flex-1 min-w-0">
                          <h4 className="text-xs font-extrabold text-slate-800 truncate hover:text-[#4648d4] cursor-pointer" onClick={() => loadEventDetails(b.eventId)}>
                            {b.title}
                          </h4>
                          <p className="text-[9px] text-slate-400 font-bold">
                            {new Date(b.date).toLocaleDateString()} at {new Date(b.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </p>
                          <div className="flex items-center gap-1 text-[9px] text-slate-400 font-bold">
                            <span className="material-symbols-outlined text-[12px] text-[#4648d4]">location_on</span>
                            <span className="truncate">{b.location}</span>
                          </div>
                        </div>

                        <div className="flex items-center gap-1.5 shrink-0">
                          <button
                            onClick={() => loadEventDetails(b.eventId)}
                            className="px-2 py-1 bg-white border border-slate-200 text-slate-600 text-[9px] font-bold rounded"
                          >
                            Details
                          </button>
                          <button
                            onClick={() => handleLeaveEvent(b.eventId)}
                            className="w-7 h-7 rounded bg-red-50 text-red-600 flex items-center justify-center hover:bg-red-100"
                          >
                            <span className="material-symbols-outlined text-base">logout</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

            </div>
          </div>
        )}

        {/* VIEW: ADMIN CONTROL PANEL */}
        {currentView === 'admin' && user?.role === 'Admin' && (
          <div className="space-y-6 animate-fade-in text-left">
            <div>
              <h2 className="text-xl font-extrabold tracking-tight text-slate-900">GatherPulse Control Panel</h2>
              <p className="text-xs text-slate-400 font-semibold mt-0.5">Manage custom categories, review user accounts, and moderate platform content.</p>
            </div>

            <div className="flex items-center gap-2 border-b border-slate-200 pb-1">
              <button
                onClick={() => setAdminTab('categories')}
                className={`px-4 py-2 border-b-2 font-bold text-xs transition-colors flex items-center gap-1.5 ${
                  adminTab === 'categories' ? 'border-[#4648d4] text-[#4648d4]' : 'border-transparent text-slate-400 hover:text-slate-800'
                }`}
              >
                Categories CRUD
              </button>
              <button
                onClick={() => setAdminTab('users')}
                className={`px-4 py-2 border-b-2 font-bold text-xs transition-colors flex items-center gap-1.5 ${
                  adminTab === 'users' ? 'border-[#4648d4] text-[#4648d4]' : 'border-transparent text-slate-400 hover:text-slate-800'
                }`}
              >
                User Accounts
              </button>
              <button
                onClick={() => setAdminTab('moderation')}
                className={`px-4 py-2 border-b-2 font-bold text-xs transition-colors flex items-center gap-1.5 ${
                  adminTab === 'moderation' ? 'border-[#4648d4] text-[#4648d4]' : 'border-transparent text-slate-400 hover:text-slate-800'
                }`}
              >
                Mod Moderation
              </button>
            </div>

            {/* TAB: CATEGORIES CRUD */}
            {adminTab === 'categories' && (
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                <div className="lg:col-span-1 bg-white rounded-2xl p-6 border border-slate-200 shadow-xs space-y-4">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider">
                    {editingCategoryId ? 'Edit Category' : 'Create Category'}
                  </h3>
                  
                  <form onSubmit={editingCategoryId ? handleUpdateCategory : handleCreateCategory} className="space-y-4">
                    <div className="space-y-1.5">
                      <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Category Name *</label>
                      <input
                        type="text"
                        required
                        placeholder="e.g. Wellness"
                        value={editingCategoryId ? editingCategoryName : newCategoryName}
                        onChange={(e) => editingCategoryId ? setEditingCategoryName(e.target.value) : setNewCategoryName(e.target.value)}
                        className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                      />
                    </div>

                    <div className="space-y-1.5">
                      <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Description</label>
                      <textarea
                        rows={3}
                        placeholder="Purpose of this category..."
                        value={editingCategoryId ? editingCategoryDesc : newCategoryDesc}
                        onChange={(e) => editingCategoryId ? setEditingCategoryDesc(e.target.value) : setNewCategoryDesc(e.target.value)}
                        className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                      />
                    </div>

                    <div className="flex items-center gap-2 pt-2">
                      {editingCategoryId && (
                        <button
                          type="button"
                          onClick={() => {
                            setEditingCategoryId(null);
                            setEditingCategoryName('');
                            setEditingCategoryDesc('');
                          }}
                          className="flex-1 py-2 bg-slate-100 text-slate-600 text-xs font-bold rounded-lg"
                        >
                          Cancel
                        </button>
                      )}
                      <button
                        type="submit"
                        className="flex-1 py-2 bg-[#4648d4] hover:bg-[#3738bd] text-white text-xs font-bold rounded-lg shadow-sm"
                      >
                        {editingCategoryId ? 'Save' : 'Create'}
                      </button>
                    </div>
                  </form>
                </div>

                <div className="lg:col-span-2 bg-white rounded-2xl p-6 border border-slate-200 shadow-xs space-y-4">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider">Categories</h3>
                  <div className="overflow-x-auto border border-slate-100 rounded-2xl">
                    <table className="w-full text-left border-collapse text-xs">
                      <thead>
                        <tr className="bg-slate-50 text-slate-400 font-bold border-b border-slate-100">
                          <th className="p-3">Name</th>
                          <th className="p-3">Description</th>
                          <th className="p-3 text-right">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100">
                        {categories.map((c) => (
                          <tr key={c.id} className="hover:bg-slate-50/50 font-medium">
                            <td className="p-3 font-extrabold text-slate-800">{c.name}</td>
                            <td className="p-3 text-slate-500 max-w-xs truncate">{c.description || <span className="italic opacity-60">No description</span>}</td>
                            <td className="p-3 text-right flex items-center justify-end gap-1.5">
                              <button
                                onClick={() => {
                                  setEditingCategoryId(c.id);
                                  setEditingCategoryName(c.name);
                                  setEditingCategoryDesc(c.description || '');
                                }}
                                className="w-7 h-7 rounded bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 hover:text-slate-700"
                              >
                                <span className="material-symbols-outlined text-base">edit</span>
                              </button>
                              <button
                                onClick={() => handleDeleteCategory(c.id)}
                                className="w-7 h-7 rounded bg-red-50 text-red-600 flex items-center justify-center hover:bg-red-100"
                              >
                                <span className="material-symbols-outlined text-base">delete</span>
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}

            {/* TAB: USER ACCOUNTS */}
            {adminTab === 'users' && (
              <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-xs space-y-4">
                <div className="flex items-center justify-between">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider">User Accounts</h3>
                  <button onClick={fetchAdminData} className="text-xs text-[#4648d4] font-bold hover:underline flex items-center gap-1">
                    <span className="material-symbols-outlined text-sm">refresh</span> Refresh
                  </button>
                </div>

                <div className="overflow-x-auto border border-slate-100 rounded-2xl">
                  <table className="w-full text-left border-collapse text-xs">
                    <thead>
                      <tr className="bg-slate-50 text-slate-400 font-bold border-b border-slate-100">
                        <th className="p-3">Username</th>
                        <th className="p-3">Email Address</th>
                        <th className="p-3">System Role</th>
                        <th className="p-3">Created At</th>
                        <th className="p-3">Status</th>
                        <th className="p-3 text-right">Moderation</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {adminUsers.map((u) => (
                        <tr key={u.id} className="hover:bg-slate-50/50 font-medium text-slate-500">
                          <td className="p-3 font-extrabold text-slate-800 flex items-center gap-2">
                            <div className="w-6 h-6 rounded-full bg-slate-100 text-slate-600 flex items-center justify-center font-bold text-[10px]">
                              {u.username.charAt(0).toUpperCase()}
                            </div>
                            {u.username}
                          </td>
                          <td className="p-3">{u.email}</td>
                          <td className="p-3">
                            <span className={`px-2 py-0.5 rounded text-[10px] font-bold ${u.role === 'Admin' ? 'bg-[#4648d4]/10 text-[#4648d4]' : 'bg-slate-100 text-slate-400'}`}>
                              {u.role}
                            </span>
                          </td>
                          <td className="p-3">{new Date(u.createdAt).toLocaleDateString()}</td>
                          <td className="p-3">
                            {u.isBanned ? (
                              <span className="px-2 py-0.5 rounded bg-red-100 text-red-800 text-[10px] font-bold">Banned</span>
                            ) : (
                              <span className="px-2 py-0.5 rounded bg-green-100 text-green-800 text-[10px] font-bold">Active</span>
                            )}
                          </td>
                          <td className="p-3 text-right">
                            {!u.isBanned && u.role !== 'Admin' && (
                              <button
                                onClick={() => handleBanUser(u.id)}
                                className="px-2 py-1 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 text-[10px] font-bold"
                              >
                                Ban Account
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {/* TAB: CONTENT MODERATION */}
            {adminTab === 'moderation' && (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                
                <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-xs space-y-4">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider flex items-center gap-2">
                    <span className="material-symbols-outlined text-[#4648d4] text-lg font-bold">event_busy</span>
                    Events Force Deletion
                  </h3>
                  <div className="space-y-3 max-h-[480px] overflow-y-auto pr-1">
                    {events.map((ev) => (
                      <div key={ev.id} className="p-3 rounded-2xl bg-slate-50 border border-slate-100 flex items-center justify-between gap-4 text-xs font-medium">
                        <div className="min-w-0 flex-1">
                          <p className="font-extrabold text-slate-800 truncate">{ev.title}</p>
                          <p className="text-[9px] text-slate-400 font-bold">Host: {ev.organizerName} • Status: {ev.status}</p>
                        </div>
                        <button
                          onClick={() => handleDeleteEvent(ev.id, true)}
                          className="px-2 py-1 bg-red-50 text-red-600 hover:bg-red-100 font-bold rounded text-[9px]"
                        >
                          Force Delete
                        </button>
                      </div>
                    ))}
                  </div>
                </div>

                <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-xs space-y-4">
                  <h3 className="text-xs font-extrabold text-slate-800 uppercase tracking-wider flex items-center gap-2">
                    <span className="material-symbols-outlined text-[#4648d4] text-lg font-bold">comment_bank</span>
                    Chat Board Moderation
                  </h3>
                  <p className="text-xs text-slate-400 leading-normal font-semibold">
                    To moderate comments, visit the specific Event Details page. As an Admin, option buttons to delete comments from any author will automatically show in the live feed.
                  </p>
                  <div className="p-4 bg-slate-50 border border-slate-200 rounded-2xl text-xs space-y-2">
                    <h4 className="font-bold text-slate-800">Step-by-step:</h4>
                    <ul className="list-disc list-inside text-[11px] text-slate-500 space-y-1 font-semibold">
                      <li>Go to Explore page.</li>
                      <li>Click Details on any event.</li>
                      <li>Review comments in live panel.</li>
                      <li>Click the Trash icon next to offending messages.</li>
                    </ul>
                  </div>
                </div>

              </div>
            )}
          </div>
        )}

      </main>

      {/* Footer */}
      <footer className="border-t border-slate-200/60 bg-white py-8 px-6 mt-auto shrink-0">
        <div className="max-w-7xl mx-auto flex flex-col items-center justify-center gap-2 text-xs text-slate-400 font-bold text-center">
          <h4 className="text-sm font-extrabold text-slate-800">GatherPulse</h4>
          <p className="font-medium text-[10px]">© 2026 GatherPulse Inc. Community First.</p>
        </div>
      </footer>

      {/* Componentized Auth Modal */}
      <AuthModal
        show={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        activeTab={authTab}
        onTabChange={setAuthTab}
        
        loginEmail={loginEmail}
        onLoginEmailChange={setLoginEmail}
        loginPassword={loginPassword}
        onLoginPasswordChange={setLoginPassword}
        onLoginSubmit={handleLogin}

        registerUsername={registerUsername}
        onRegisterUsernameChange={setRegisterUsername}
        registerEmail={registerEmail}
        onRegisterEmailChange={setRegisterEmail}
        registerPassword={registerPassword}
        onRegisterPasswordChange={setRegisterPassword}
        registerConfirmPassword={registerConfirmPassword}
        onRegisterConfirmPasswordChange={setRegisterConfirmPassword}
        onRegisterSubmit={handleRegister}
      />

    </div>
  );
}
