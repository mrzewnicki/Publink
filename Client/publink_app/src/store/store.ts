import { configureStore } from '@reduxjs/toolkit';
import logsReducer from '../features/logs/logsSlice';

export const store = configureStore({
  reducer: {
    logs: logsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
