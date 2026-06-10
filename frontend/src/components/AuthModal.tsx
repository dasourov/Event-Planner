import React from 'react';

interface AuthModalProps {
  show: boolean;
  onClose: () => void;
  activeTab: 'login' | 'register';
  onTabChange: (tab: 'login' | 'register') => void;

  // Login input bindings
  loginEmail: string;
  onLoginEmailChange: (val: string) => void;
  loginPassword: string;
  onLoginPasswordChange: (val: string) => void;
  onLoginSubmit: (e: React.FormEvent) => void;

  // Register input bindings
  registerUsername: string;
  onRegisterUsernameChange: (val: string) => void;
  registerEmail: string;
  onRegisterEmailChange: (val: string) => void;
  registerPassword: string;
  onRegisterPasswordChange: (val: string) => void;
  registerConfirmPassword: string;
  onRegisterConfirmPasswordChange: (val: string) => void;
  onRegisterSubmit: (e: React.FormEvent) => void;
}

export const AuthModal: React.FC<AuthModalProps> = ({
  show,
  onClose,
  activeTab,
  onTabChange,

  loginEmail,
  onLoginEmailChange,
  loginPassword,
  onLoginPasswordChange,
  onLoginSubmit,

  registerUsername,
  onRegisterUsernameChange,
  registerEmail,
  onRegisterEmailChange,
  registerPassword,
  onRegisterPasswordChange,
  registerConfirmPassword,
  onRegisterConfirmPasswordChange,
  onRegisterSubmit
}) => {
  if (!show) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-xs animate-fade-in">
      <div className="bg-white rounded-3xl max-w-sm w-full overflow-hidden border border-slate-200 shadow-2xl relative">
        
        {/* Dedicated Modal Header */}
        <div className="flex items-center justify-between px-6 py-3.5 border-b border-slate-100 bg-slate-50/50">
          <span className="text-[9px] font-bold text-slate-400 uppercase tracking-widest">Account Access</span>
          <button
            onClick={onClose}
            className="w-7 h-7 rounded-lg hover:bg-slate-200/60 flex items-center justify-center text-slate-400 hover:text-slate-700 transition-colors"
          >
            <span className="material-symbols-outlined text-base leading-none">close</span>
          </button>
        </div>

        {/* Tab Selector */}
        <div className="flex border-b border-slate-100 bg-white">
          <button
            onClick={() => onTabChange('login')}
            className={`flex-1 py-3 text-xs font-extrabold border-b-2 transition-colors ${activeTab === 'login' ? 'border-[#4648d4] text-[#4648d4] bg-white' : 'border-transparent text-slate-400 hover:text-slate-700'
              }`}
          >
            Log In
          </button>
          <button
            onClick={() => onTabChange('register')}
            className={`flex-1 py-3 text-xs font-extrabold border-b-2 transition-colors ${activeTab === 'register' ? 'border-[#4648d4] text-[#4648d4] bg-white' : 'border-transparent text-slate-400 hover:text-slate-700'
              }`}
          >
            Register
          </button>
        </div>

        <div className="p-6">
          {/* Tab 1: Login Form */}
          {activeTab === 'login' && (
            <form onSubmit={onLoginSubmit} className="space-y-4">
              <div className="text-left space-y-1">
                <h3 className="text-sm font-extrabold text-slate-800 uppercase tracking-wider">Welcome Back</h3>
                <p className="text-[10px] text-slate-400 font-bold">Provide credentials to access profiles.</p>
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="e.g. user@eventplanner.com"
                  value={loginEmail}
                  onChange={(e) => onLoginEmailChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Password</label>
                <input
                  type="password"
                  required
                  placeholder="••••••••"
                  value={loginPassword}
                  onChange={(e) => onLoginPasswordChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <button
                type="submit"
                className="w-full py-2.5 bg-[#4648d4] hover:bg-[#3738bd] text-white font-bold rounded-xl text-xs uppercase tracking-wider transition-colors shadow-sm"
              >
                Log In
              </button>
            </form>
          )}

          {/* Tab 2: Register Form */}
          {activeTab === 'register' && (
            <form onSubmit={onRegisterSubmit} className="space-y-4">
              <div className="text-left space-y-1">
                <h3 className="text-sm font-extrabold text-slate-800 uppercase tracking-wider">Register Profile</h3>
                <p className="text-[10px] text-slate-400 font-bold">Create a community profile handle.</p>
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Username</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. gatherer99"
                  value={registerUsername}
                  onChange={(e) => onRegisterUsernameChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="e.g. join@gatherpulse.com"
                  value={registerEmail}
                  onChange={(e) => onRegisterEmailChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Password</label>
                <input
                  type="password"
                  required
                  placeholder="Min 6 characters, uppercase, digit"
                  value={registerPassword}
                  onChange={(e) => onRegisterPasswordChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Confirm Password</label>
                <input
                  type="password"
                  required
                  placeholder="Confirm entry"
                  value={registerConfirmPassword}
                  onChange={(e) => onRegisterConfirmPasswordChange(e.target.value)}
                  className="w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border border-slate-100 focus:outline-none focus:bg-white focus:ring-1 focus:ring-[#4648d4] text-xs"
                />
              </div>

              <button
                type="submit"
                className="w-full py-2.5 bg-[#4648d4] hover:bg-[#3738bd] text-white font-bold rounded-xl text-xs uppercase tracking-wider transition-colors shadow-sm"
              >
                Register
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};
