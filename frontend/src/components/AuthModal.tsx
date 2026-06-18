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
  registerIsOrganizer: boolean;
  onRegisterIsOrganizerChange: (val: boolean) => void;
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
  registerIsOrganizer,
  onRegisterIsOrganizerChange,
  onRegisterSubmit
}) => {
  // Client-side username length validation
  const isUsernameTooShort = registerUsername.length > 0 && registerUsername.length < 3;

  // Client-side email validation regex
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const isEmailInvalid = registerEmail.length > 0 && !emailRegex.test(registerEmail);

  // Password Requirements Checker
  const hasMinLength = registerPassword.length >= 8;
  const hasUpper = /[A-Z]/.test(registerPassword);
  const hasLower = /[a-z]/.test(registerPassword);
  const hasNumber = /[0-9]/.test(registerPassword);
  const isPasswordValid = hasMinLength && hasUpper && hasLower && hasNumber;

  // Confirm Password Check
  const confirmMatch = registerConfirmPassword.length > 0 && registerPassword === registerConfirmPassword;

  // Form Validation State (Instant Client-side)
  const isFormValid =
    registerUsername.length >= 3 &&
    emailRegex.test(registerEmail) &&
    isPasswordValid &&
    confirmMatch;

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

              {/* Username Input with Length Check */}
              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Username</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. gatherer99"
                  value={registerUsername}
                  onChange={(e) => onRegisterUsernameChange(e.target.value)}
                  className={`w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border focus:outline-none focus:bg-white focus:ring-1 text-xs ${registerUsername.length > 0 && !isUsernameTooShort
                      ? 'border-emerald-200 focus:ring-emerald-500'
                      : isUsernameTooShort
                        ? 'border-rose-200 focus:ring-rose-500'
                        : 'border-slate-100 focus:ring-[#4648d4]'
                    }`}
                />
                {isUsernameTooShort && (
                  <p className="text-[10px] text-rose-500 font-bold flex items-center gap-0.5 mt-0.5">
                    <span className="material-symbols-outlined text-[13px]">warning</span> Must be at least 3 characters
                  </p>
                )}
              </div>

              {/* Email Input with Format check */}
              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="e.g. join@gatherpulse.com"
                  value={registerEmail}
                  onChange={(e) => onRegisterEmailChange(e.target.value)}
                  className={`w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border focus:outline-none focus:bg-white focus:ring-1 text-xs ${registerEmail.length > 0 && !isEmailInvalid
                      ? 'border-emerald-200 focus:ring-emerald-500'
                      : isEmailInvalid
                        ? 'border-rose-200 focus:ring-rose-500'
                        : 'border-slate-100 focus:ring-[#4648d4]'
                    }`}
                />
                {isEmailInvalid && (
                  <p className="text-[10px] text-rose-500 font-bold flex items-center gap-0.5 mt-0.5">
                    <span className="material-symbols-outlined text-[13px]">mail</span> Please enter a valid email address
                  </p>
                )}
              </div>

              {/* Account Type Choice */}
              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Account Type</label>
                <div className="grid grid-cols-2 gap-2 mt-1">
                  <button
                    type="button"
                    onClick={() => onRegisterIsOrganizerChange(false)}
                    className={`py-2 text-[10px] font-extrabold rounded-xl border transition-all ${!registerIsOrganizer
                        ? 'bg-[#4648d4]/10 border-[#4648d4] text-[#4648d4]'
                        : 'bg-slate-50 border-slate-100 text-slate-400 hover:text-slate-600'
                      }`}
                  >
                    Attendee
                  </button>
                  <button
                    type="button"
                    onClick={() => onRegisterIsOrganizerChange(true)}
                    className={`py-2 text-[10px] font-extrabold rounded-xl border transition-all ${registerIsOrganizer
                        ? 'bg-[#4648d4]/10 border-[#4648d4] text-[#4648d4]'
                        : 'bg-slate-50 border-slate-100 text-slate-400 hover:text-slate-600'
                      }`}
                  >
                    Organizer
                  </button>
                </div>
              </div>

              {/* Password Input with Requirements Checklist */}
              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Password</label>
                <input
                  type="password"
                  required
                  placeholder="••••••••"
                  value={registerPassword}
                  onChange={(e) => onRegisterPasswordChange(e.target.value)}
                  className={`w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border focus:outline-none focus:bg-white focus:ring-1 text-xs ${registerPassword.length > 0
                      ? isPasswordValid
                        ? 'border-emerald-200 focus:ring-emerald-500'
                        : 'border-rose-200 focus:ring-rose-500'
                      : 'border-slate-100 focus:ring-[#4648d4]'
                    }`}
                />
                {registerPassword.length > 0 && (
                  <div className="p-2.5 bg-slate-50 border border-slate-100 rounded-xl space-y-1 mt-1">
                    <p className="text-[9px] font-extrabold text-slate-400 uppercase tracking-wider">Strength Requirements</p>
                    <div className="grid grid-cols-2 gap-x-3 gap-y-1 text-[10px]">
                      <span className={`flex items-center gap-1 font-bold ${hasMinLength ? 'text-emerald-600' : 'text-slate-400'}`}>
                        <span className="material-symbols-outlined text-[11px] font-bold">{hasMinLength ? 'done' : 'circle'}</span> 8+ characters
                      </span>
                      <span className={`flex items-center gap-1 font-bold ${hasUpper ? 'text-emerald-600' : 'text-slate-400'}`}>
                        <span className="material-symbols-outlined text-[11px] font-bold">{hasUpper ? 'done' : 'circle'}</span> Uppercase letter
                      </span>
                      <span className={`flex items-center gap-1 font-bold ${hasLower ? 'text-emerald-600' : 'text-slate-400'}`}>
                        <span className="material-symbols-outlined text-[11px] font-bold">{hasLower ? 'done' : 'circle'}</span> Lowercase letter
                      </span>
                      <span className={`flex items-center gap-1 font-bold ${hasNumber ? 'text-emerald-600' : 'text-slate-400'}`}>
                        <span className="material-symbols-outlined text-[11px] font-bold">{hasNumber ? 'done' : 'circle'}</span> Digit (0-9)
                      </span>
                    </div>
                  </div>
                )}
              </div>

              {/* Confirm Password Input with Match Indicator */}
              <div className="space-y-1 text-left">
                <label className="text-[9px] font-bold text-slate-400 uppercase tracking-wider">Confirm Password</label>
                <input
                  type="password"
                  required
                  placeholder="Confirm entry"
                  value={registerConfirmPassword}
                  onChange={(e) => onRegisterConfirmPasswordChange(e.target.value)}
                  className={`w-full p-2.5 bg-slate-50 text-slate-800 rounded-xl border focus:outline-none focus:bg-white focus:ring-1 text-xs ${registerConfirmPassword.length > 0
                      ? confirmMatch
                        ? 'border-emerald-200 focus:ring-emerald-500'
                        : 'border-rose-200 focus:ring-rose-500'
                      : 'border-slate-100 focus:ring-[#4648d4]'
                    }`}
                />
                {registerConfirmPassword.length > 0 && (
                  confirmMatch ? (
                    <p className="text-[10px] text-emerald-600 font-bold flex items-center gap-0.5 mt-0.5">
                      <span className="material-symbols-outlined text-[13px]">check_circle</span> Passwords match
                    </p>
                  ) : (
                    <p className="text-[10px] text-rose-500 font-bold flex items-center gap-0.5 mt-0.5">
                      <span className="material-symbols-outlined text-[13px]">error</span> Passwords do not match
                    </p>
                  )
                )}
              </div>

              {/* Submit Button */}
              <button
                type="submit"
                disabled={!isFormValid}
                className={`w-full py-2.5 font-bold rounded-xl text-xs uppercase tracking-wider transition-colors shadow-sm ${isFormValid
                    ? 'bg-[#4648d4] hover:bg-[#3738bd] text-white cursor-pointer'
                    : 'bg-slate-100 text-slate-400 cursor-not-allowed border border-slate-200/50'
                  }`}
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
