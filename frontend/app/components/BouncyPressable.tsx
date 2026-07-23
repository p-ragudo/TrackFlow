import React, { useRef } from 'react';
import { Animated, Pressable, PressableProps, StyleProp, ViewStyle } from 'react-native';

interface BouncyPressableProps extends PressableProps {
  children: React.ReactNode;
  style?: StyleProp<ViewStyle>;
  scaleTo?: number; // Target scale down ratio (default 0.95)
}

export const BouncyPressable: React.FC<BouncyPressableProps> = ({
  children,
  style,
  scaleTo = 0.96,
  onPressIn,
  onPressOut,
  ...props
}) => {
  const scaleValue = useRef(new Animated.Value(1)).current;

  const handlePressIn = (e: any) => {
    Animated.spring(scaleValue, {
      toValue: scaleTo,
      useNativeDriver: true,
      speed: 100, // Fast contraction
      bounciness: 0,
    }).start();

    if (onPressIn) onPressIn(e);
  };

  const handlePressOut = (e: any) => {
    Animated.spring(scaleValue, {
      toValue: 1,
      useNativeDriver: true,
      speed: 20,
      bounciness: 0,
    }).start();

    if (onPressOut) onPressOut(e);
  };

  return (
    <Animated.View style={{ transform: [{ scale: scaleValue }] }}>
      <Pressable
        {...props}
        style={style}
        onPressIn={handlePressIn}
        onPressOut={handlePressOut}
      >
        {children}
      </Pressable>
    </Animated.View>
  );
};