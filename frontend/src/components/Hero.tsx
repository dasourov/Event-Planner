import React from 'react';

export const Hero: React.FC = () => {
  return (
    <div className="space-y-3.5 text-left max-w-3xl py-2">
      <h2 className="text-3xl md:text-5xl font-extrabold tracking-tight text-slate-900 leading-tight">
        Discover your next <span className="text-[#4648d4]">experience.</span>
      </h2>
      <p className="text-xs sm:text-sm text-slate-500 font-medium leading-relaxed max-w-xl">
        Connect with communities through workshops, networking, and creative gatherings tailored for you.
      </p>
    </div>
  );
};
