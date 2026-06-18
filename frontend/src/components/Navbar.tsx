import React from 'react';

interface UserProfile {
  id: string;
  username: string;
  email: string;
  role: string;
}

interface NavbarProps {
  user: UserProfile | null;
  token: string | null;
  onLogout: () => void;
  onShowAuth: (tab: 'login' | 'register') => void;
  currentView: string;
  onNavigate: (view: 'home' | 'explore' | 'my-events' | 'admin') => void;
  searchTerm: string;
  onSearchChange: (val: string) => void;
  onCreateEventClick: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  user,
  token,
  onLogout,
  onShowAuth,
  currentView,
  onNavigate,
  searchTerm,
  onSearchChange,
  onCreateEventClick
}) => {
  return (
    <header className="sticky top-0 z-40 bg-white border-b border-slate-200/50 px-6 py-5 md:py-6 shadow-xs">
      <div className="max-w-7xl mx-auto flex items-center justify-between gap-4">
        
        {/* Left: GatherPulse Branding & Links */}
        <div className="flex items-center gap-8">
          <div 
            onClick={() => onNavigate('home')}
            className="cursor-pointer select-none"
          >
            <h1 className="text-xl font-extrabold tracking-tight text-[#4648d4]">
              GatherPulse
            </h1>
          </div>
          
          <nav className="hidden md:flex items-center gap-6">
            {token && user?.role !== 'Admin' && (
              <button
                onClick={() => onNavigate('my-events')}
                className={`text-xs font-semibold tracking-wide transition-colors ${
                  currentView === 'my-events' ? 'text-[#4648d4]' : 'text-slate-500 hover:text-slate-800'
                }`}
              >
                My Events
              </button>
            )}
            {token && user?.role === 'Admin' && (
              <button
                onClick={() => onNavigate('admin')}
                className={`text-xs font-semibold tracking-wide transition-colors ${
                  currentView === 'admin' ? 'text-[#4648d4]' : 'text-slate-500 hover:text-slate-800'
                }`}
              >
                Admin Panel
              </button>
            )}
          </nav>
        </div>

        {/* Right: Search, Notifications, Auth and Action Button */}
        <div className="flex items-center gap-4">
          
          {/* Header Search bar (Only active on explore) */}
          {currentView === 'explore' && (
            <div className="relative w-40 sm:w-60">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-lg">search</span>
              <input
                type="text"
                placeholder="Search events..."
                value={searchTerm}
                onChange={(e) => onSearchChange(e.target.value)}
                className="w-full pl-9 pr-8 py-2 bg-slate-50 text-slate-800 text-xs rounded-full border border-slate-200 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4]/50 transition-all placeholder:text-slate-400"
              />
              {searchTerm && (
                <button
                  onClick={() => onSearchChange('')}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center text-slate-400 hover:text-slate-600"
                >
                  <span className="material-symbols-outlined text-base">close</span>
                </button>
              )}
            </div>
          )}

          {/* User profile dropdown avatar or Sign In and Login buttons */}
          {user ? (
            <div className="flex items-center gap-2">
              <div 
                className="w-9 h-9 rounded-full bg-slate-100 border border-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs uppercase cursor-default select-none"
                title={`${user.username} (${user.role})`}
              >
                {user.username.charAt(0)}
              </div>
              <button
                onClick={onLogout}
                className="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                title="Log Out"
              >
                <span className="material-symbols-outlined text-lg">logout</span>
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <button
                onClick={() => onShowAuth('login')}
                className="px-3 py-2 text-slate-500 hover:text-slate-800 text-xs font-semibold rounded-lg transition-colors"
              >
                Log In
              </button>
              <button
                onClick={() => onShowAuth('register')}
                className="px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold rounded-lg transition-all"
              >
                Register
              </button>
            </div>
          )}

          {/* Solid Create Button */}
          {(!user || user.role === 'Organizer' || user.role === 'Admin') && (
            <button
              onClick={onCreateEventClick}
              className="px-4 py-2 bg-[#4648d4] hover:bg-[#3738bd] text-white text-xs font-bold rounded-lg shadow-sm hover:shadow transition-all active:scale-95 flex items-center gap-1"
            >
              Create Event
            </button>
          )}

        </div>
      </div>
    </header>
  );
};
