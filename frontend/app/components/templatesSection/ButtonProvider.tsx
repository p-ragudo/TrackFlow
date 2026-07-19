import React, { createContext, useContext, useState, ReactNode } from 'react';

interface ButtonContextType {
  isAnyButtonBusy: boolean;
  triggerAction: (actionFn: () => Promise<void> | void) => Promise<void>;
}

const ButtonContext = createContext<ButtonContextType | undefined>(undefined);

export const ButtonProvider = ({ children }: { children: ReactNode }) => {
  const [isAnyButtonBusy, setIsAnyButtonBusy] = useState(false);

  // Wraps any button action: disables everything, runs it, then re-enables
  const triggerAction = async (actionFn: () => Promise<void> | void) => {
    if (isAnyButtonBusy) return; 
    
    setIsAnyButtonBusy(true);
    try {
      await actionFn(); // Waits for your API call, navigation, etc. to finish
    } finally {
      setIsAnyButtonBusy(false); // Re-enables all buttons automatically
    }
  };

  return (
    <ButtonContext.Provider value={{ isAnyButtonBusy, triggerAction }}>
      {children}
    </ButtonContext.Provider>
  );
};

export const useGlobalButtons = () => {
  const context = useContext(ButtonContext);
  if (!context) {
    throw new Error('useGlobalButtons must be used within a ButtonProvider');
  }
  return context;
};