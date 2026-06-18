import React from 'react';

interface Category {
  id: string;
  name: string;
  description: string;
}

interface CategoryFiltersProps {
  categories: Category[];
  selectedCategoryId: string;
  onSelectCategory: (id: string) => void;
}

export const CategoryFilters: React.FC<CategoryFiltersProps> = ({
  categories,
  selectedCategoryId,
  onSelectCategory
}) => {
  
  // Icon mapping helper
  const getCategoryIcon = (catName: string) => {
    const name = catName?.toLowerCase() || '';
    if (name.includes('tech')) return 'laptop';
    if (name.includes('sport')) return 'sports_soccer';
    if (name.includes('music')) return 'music_note';
    if (name.includes('art')) return 'palette';
    if (name.includes('networking')) return 'groups';
    if (name.includes('wellness')) return 'spa';
    if (name.includes('workshop')) return 'build';
    if (name.includes('food')) return 'restaurant';
    return 'label'; // Default icon
  };

  return (
    <div className="flex items-center gap-2.5 overflow-x-auto pb-1.5 scrollbar-none">
      <button
        onClick={() => onSelectCategory('')}
        className={`px-4 py-2.5 rounded-full text-xs font-bold flex items-center gap-1.5 transition-all shrink-0 ${
          selectedCategoryId === ''
            ? 'bg-[#4648d4] text-white shadow-sm shadow-[#4648d4]/10'
            : 'bg-white text-slate-600 hover:bg-slate-50 border border-slate-200'
        }`}
      >
        <span className="material-symbols-outlined text-sm">grid_view</span>
        All Events
      </button>

      {categories.map((c) => {
        const icon = getCategoryIcon(c.name);
        const isActive = selectedCategoryId === c.id;
        return (
          <button
            key={c.id}
            onClick={() => onSelectCategory(c.id)}
            className={`px-4 py-2.5 rounded-full text-xs font-bold flex items-center gap-1.5 transition-all shrink-0 ${
              isActive
                ? 'bg-[#4648d4] text-white shadow-sm shadow-[#4648d4]/10'
                : 'bg-white text-slate-600 hover:bg-slate-50 border border-slate-200'
            }`}
          >
            <span className={`material-symbols-outlined text-sm ${isActive ? 'text-white' : 'text-[#4648d4]'}`}>{icon}</span>
            {c.name}
          </button>
        );
      })}
    </div>
  );
};
