import React, { createContext, useContext, useState, ReactNode, Dispatch, SetStateAction } from 'react';

const TEST_SPREADSHEET_ID = process.env.EXPO_PUBLIC_TEST_USER_SPREADSHEET_ID || ""
const PROD_SPREADSHEET_ID = process.env.EXPO_PUBLIC_SPREADSHEET_ID || ""

interface TestUserContextType {
    isForTestUser: boolean
    setIsForTestUser: Dispatch<SetStateAction<boolean>>;
    activeSpreadsheetId: string
}

const TestUserContext = createContext<TestUserContextType | undefined>(undefined)

export const TestUserProvider = ({ children }: { children: ReactNode }) => {
    const [isForTestUser, setIsForTestUser] = useState(false)
    const activeSpreadsheetId = isForTestUser
        ?  TEST_SPREADSHEET_ID
        : PROD_SPREADSHEET_ID

    return (
        <TestUserContext.Provider value={{ isForTestUser, setIsForTestUser, activeSpreadsheetId }}>
            {children}
        </TestUserContext.Provider>
    )
}

export const useTestUser = () => {
    const context = useContext(TestUserContext)

    if (!context) {
        throw new Error('useTestUser must be used within a ButtonProvider');
    }
  return context;
}