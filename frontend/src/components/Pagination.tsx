import React from 'react';

interface PaginationProps {
  activePage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({
  activePage,
  totalPages,
  onPageChange
}) => {
  if (totalPages <= 1) return null;

  const getPages = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;
    
    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);
      
      const start = Math.max(2, activePage - 1);
      const end = Math.min(totalPages - 1, activePage + 1);
      
      if (start > 2) {
        pages.push('...');
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (end < totalPages - 1) {
        pages.push('...');
      }
      
      // Always show last page
      pages.push(totalPages);
    }
    return pages;
  };

  const pages = getPages();

  return (
    <div className="flex items-center justify-center gap-1.5 pt-6">
      <button 
        onClick={() => activePage > 1 && onPageChange(activePage - 1)}
        disabled={activePage === 1}
        className={`w-8 h-8 rounded-lg border border-slate-200 flex items-center justify-center text-slate-400 ${
          activePage === 1 ? 'opacity-50 cursor-not-allowed' : 'hover:bg-slate-50 cursor-pointer'
        }`}
      >
        <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
      </button>

      {pages.map((page, idx) => {
        if (page === '...') {
          return (
            <span key={`ell-${idx}`} className="px-1.5 text-slate-400 text-xs font-bold leading-none select-none">
              ...
            </span>
          );
        }
        
        const pageNum = page as number;
        const isActive = activePage === pageNum;
        
        return (
          <button
            key={`page-${pageNum}`}
            onClick={() => onPageChange(pageNum)}
            className={`w-8 h-8 rounded-lg font-extrabold text-xs flex items-center justify-center border transition-all ${
              isActive 
                ? 'bg-[#4648d4] text-white border-[#4648d4]' 
                : 'border-slate-200 hover:bg-slate-50 text-slate-600'
            }`}
          >
            {pageNum}
          </button>
        );
      })}

      <button 
        onClick={() => activePage < totalPages && onPageChange(activePage + 1)}
        disabled={activePage === totalPages}
        className={`w-8 h-8 rounded-lg border border-slate-200 flex items-center justify-center text-slate-400 ${
          activePage === totalPages ? 'opacity-50 cursor-not-allowed' : 'hover:bg-slate-50 cursor-pointer'
        }`}
      >
        <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
      </button>
    </div>
  );
};
