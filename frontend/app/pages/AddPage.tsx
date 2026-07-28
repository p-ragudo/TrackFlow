import React, { useState } from 'react';
import { StyleSheet, Text, TextInput, View, ScrollView, Pressable } from 'react-native';
import CreatableSelect from '../components/CreatableSelect';
import { BouncyPressable } from '../components/BouncyPressable';
import Animated, { useAnimatedStyle, withTiming } from 'react-native-reanimated';
import LoadingOverlay from "../components/LoadingOverlay"
import { useTestUser } from '../context/TestUserContext';

interface AddPageProps {
    title: string
    onCancelPressed: () => void
    onSavePressed: (data: FormData, type: AddPageType) => Promise<void>
    groups: string[]
    categories: string[]
    tags: string[]
    type: AddPageType
}

export interface FormData {
    name: string
    group: string
    category: string
    tag: string
    amount: string
    description: string
}

export type AddPageType = 'expenses' | 'templates' | 'savings' 

export default function AddPage({ 
    title, 
    onCancelPressed, 
    onSavePressed, 
    groups, 
    categories, 
    tags ,
    type
}: AddPageProps) {
    const { isForTestUser } = useTestUser()

    const [form, setForm] = useState<FormData>({
        name: '',
        group: '',
        category: '',
        tag: '',
        amount: '',
        description: ''
    })
    const [isSaving, setIsSaving] = useState(false)

    const handleChange = (field: string, value: string) => {
        setForm((prev) => ({...prev, [field]: value}))
    }

    const handleOnSavePressed = async () => {
        setIsSaving(true)
        try {
            await onSavePressed(form, type)
        } finally {
            setIsSaving(false)
        }
    }

    const backdropAnimatedStyle = useAnimatedStyle(() => ({
        opacity: withTiming(isSaving ? 1 : 0, { duration: 200 }),
        pointerEvents: isSaving ? ('auto' as const) : ('none' as const),
    }));

    return (
        <View style={{ flex: 1 }}>
            <ScrollView 
                style={styles.page}
                contentContainerStyle={styles.scrollContent}
                keyboardShouldPersistTaps="handled" // Crucial for inner component touches/scrolls
                nestedScrollEnabled={true}
            >
                <View style={styles.header}>
                    <Text style={styles.headerText}>{title}</Text>
                    <BouncyPressable
                        style={styles.cancelButton}
                        onPress={onCancelPressed}
                    >
                        <Text style={styles.cancelButtonText}>Cancel</Text>
                    </BouncyPressable>
                </View>

                <View style={[styles.form, styles.formGap]}>
                    <View>
                        <Text style={styles.inputLabel}>Name</Text>
                        <TextInput 
                            style={styles.input}
                            value={form.name}
                            onChangeText={(val) => handleChange('name', val)}
                        />
                    </View>

                    <View style={[styles.reverseContainer, styles.formGap]}>
                        <BouncyPressable
                            onPress={handleOnSavePressed}
                            style={styles.saveButton}
                        >
                            <Text style={styles.saveButtonText}>Save</Text>
                        </BouncyPressable>

                        <View>
                            <Text style={styles.inputLabel}>Description</Text>
                            <TextInput 
                                style={styles.input}
                                value={form.description}
                                onChangeText={(val) => handleChange('description', val)}
                            />
                        </View>

                        <View>
                            <Text style={styles.inputLabel}>Amount</Text>
                            <TextInput 
                                style={styles.input}
                                value={form.amount}
                                onChangeText={(val) => handleChange('amount', val)}
                            />
                        </View>

                        { !isForTestUser && 
                            <View>
                                <CreatableSelect 
                                    label="Tag"
                                    value={form.tag}
                                    onChangeText={(val: string) => handleChange('tag', val)}
                                    options={tags}
                                />

                                <CreatableSelect 
                                    label="Category"
                                    value={form.category}
                                    onChangeText={(val: string) => handleChange('category', val)}
                                    options={categories}
                                />

                                <CreatableSelect 
                                    label="Group"
                                    value={form.group}
                                    onChangeText={(val: string) => handleChange('group', val)}
                                    options={groups}
                                />
                            </View>
                        }
                    </View>
                </View>
            </ScrollView>

            <Animated.View style={[styles.backdrop, backdropAnimatedStyle]}>
                <Pressable style={StyleSheet.absoluteFill} />
                <View style={styles.loadingContainer}>
                    <LoadingOverlay text='Processing...' />
                </View>
            </Animated.View>
        </View>
    )
}

const styles = StyleSheet.create({
    page: {
        padding: 20
    },
    scrollContent: {
        paddingBottom: 300, // Gives clean space below the last element
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between'
    },
    form: {
        marginTop: 24,
        marginBottom: 100
    },
    formGap: {
        gap: 20
    },
    headerText: {
        fontSize: 24,
        fontWeight: 500
    },
    cancelButton: {
        backgroundColor: 'black',
        paddingVertical: 8,
        paddingHorizontal: 16,
        borderRadius: 6 
    },
    cancelButtonText: {
        color: 'white',
    },
    input: {
        height: 44,
        borderWidth: 0.2,
        borderColor: '#8E8E8E',
        borderRadius: 8,
        paddingHorizontal: 12,
        fontSize: 15,
        color: '#000000',
        backgroundColor: '#FFFFFF',
    },
    inputLabel: {
        fontWeight: 600,
        marginBottom: 6,
    },
    reverseContainer: {
        flexDirection: 'column-reverse'
    },
    saveButton: {
        backgroundColor: 'black',
        alignItems: 'center',
        fontSize: 18,
        paddingVertical: 12,
        borderRadius: 8,
        marginTop: 20
    },
    saveButtonText: {
        color: 'white',
    },
    backdrop: {
        ...StyleSheet.absoluteFillObject,
        backgroundColor: 'rgba(0, 0, 0, 0.50)',
        zIndex: 10,
        justifyContent: 'center',
        alignItems: 'center'
    },
    loadingContainer: {
        justifyContent: 'center',
        alignItems: 'center',
        width: '100%',
        paddingHorizontal: 20
    }
})