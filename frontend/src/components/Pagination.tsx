import React from 'react';

interface PaginationProps {
  activePage: number;
  onPageChange: (page: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({
  activePage,
  onPageChange
}) => {
  return (
    <div className="flex items-center justify-center gap-1.5 pt-4">
      <button 
        onClick={() => activePage > 1 && onPageChange(activePage - 1)}
        className="w-8 h-8 rounded-lg border border-slate-200 hover:bg-slate-50 flex items-center justify-center text-slate-400"
      >
        <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
      </button>
      {[1, 2, 3].map((page) => (
        <button
          key={page}
          onClick={() => onPageChange(page)}
          className={`w-8 h-8 rounded-lg font-extrabold text-xs flex items-center justify-center border ${
            activePage === page 
              ? 'bg-[#4648d4] text-white border-[#4648d4]' 
              : 'border-slate-200 hover:bg-slate-50 text-slate-600'
          }`}
        >
          {page}
        </button>
      ))}
      <span className="px-1.5 text-slate-400 text-xs font-bold leading-none">...</span>
      <button
        onClick={() => onPageChange(12)}
        className={`w-8 h-8 rounded-lg font-extrabold text-xs flex items-center justify-center border ${
          activePage === 12 
            ? 'bg-[#4648d4] text-white border-[#4648d4]' 
            : 'border-slate-200 hover:bg-slate-50 text-slate-600'
        }`}
      >
        12
      </button>
      <button 
        onClick={() => activePage < 12 && onPageChange(activePage + 1)}
        className="w-8 h-8 rounded-lg border border-slate-200 hover:bg-slate-50 flex items-center justify-center text-slate-400"
      >
        <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
      </button>
    </div>
  );
};
